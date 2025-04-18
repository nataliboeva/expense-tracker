using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Models;
using Expense_Tracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Expense_Tracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string DefaultCurrency = "EUR";
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ApplicationDbContext context, ILogger<TransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Transaction
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? type, string? displayCurrency)
        {
            try
            {
                // Default to last 30 days if no dates provided
                DateTime StartDate = startDate ?? DateTime.Today.AddDays(-29);
                DateTime EndDate = endDate ?? DateTime.Today;
                string currency = displayCurrency ?? DefaultCurrency;

                ViewBag.StartDate = StartDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = EndDate.ToString("yyyy-MM-dd");
                ViewBag.DisplayCurrency = currency;
                ViewBag.Type = type;

                var query = _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date >= StartDate && t.Date <= EndDate);

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(t => t.Category.Type == type);
                }

                var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();

                // Normalize currency strings to supported values
                foreach (var transaction in transactions)
                {
                    if (transaction.Currency == "DOLLAR")
                    {
                        transaction.Currency = "USD";
                    }
                }

                // Only convert currencies if requested currency is different
                if (currency != "DOLLAR" && currency != null)
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction.Currency != currency)
                        {
                            try
                            {
                                transaction.Amount = CurrencyConverter.Convert(transaction.Amount, transaction.Currency, currency);
                                transaction.Currency = currency;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Currency conversion error");
                            }
                        }
                    }
                }

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while loading transactions: " + ex.Message
                });
            }
        }

        // GET: Transaction/AddOrEdit
        public IActionResult AddOrEdit(int id = 0)
        {
            try
            {
                // Get categories and convert to IEnumerable
                var categories = _context.Categories
                    .Where(c => c.CategoryId > 0)  // Exclude any default category
                    .OrderBy(c => c.Type)
                    .ThenBy(c => c.Title)
                    .AsEnumerable();  // Convert to IEnumerable to enable LINQ methods in view

                ViewBag.Categories = categories;

                if (id == 0)
                {
                    return View(new Transaction
                    {
                        Currency = DefaultCurrency,
                        Date = DateTime.Now
                    });
                }
                else
                {
                    var transaction = _context.Transactions
                        .AsNoTracking()
                        .Include(t => t.Category)
                        .FirstOrDefault(t => t.TransactionId == id);

                    if (transaction == null)
                    {
                        return NotFound();
                    }

                    return View(transaction);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrEdit GET");
                ModelState.AddModelError("", "An error occurred while loading the transaction form.");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while loading the transaction form: " + ex.Message
                });
            }
        }

        // POST: Transaction/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date,Currency")] Transaction transaction)
        {
            try
            {
                // Ensure proper decimal culture handling
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                // Validate CategoryId
                if (transaction.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Please select a valid category.");
                }

                // Validate Amount
                if (transaction.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Amount must be greater than 0.");
                }

                // Validate Currency
                if (string.IsNullOrEmpty(transaction.Currency))
                {
                    transaction.Currency = DefaultCurrency;
                }

                // Set date if not provided
                if (transaction.Date == default)
                {
                    transaction.Date = DateTime.Now;
                }

                if (ModelState.IsValid)
                {
                    if (transaction.TransactionId == 0)
                    {
                        _context.Add(transaction);
                    }
                    else
                    {
                        var existingTransaction = await _context.Transactions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

                        if (existingTransaction == null)
                        {
                            return NotFound();
                        }

                        // Preserve the original date if not explicitly changed
                        if (transaction.Date == default)
                        {
                            transaction.Date = existingTransaction.Date;
                        }

                        // Only convert currency if it has changed
                        if (existingTransaction.Currency != transaction.Currency)
                        {
                            transaction.Amount = CurrencyConverter.Convert(
                                transaction.Amount,
                                existingTransaction.Currency,
                                transaction.Currency);
                        }

                        _context.Update(transaction);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed, redisplay form
                var categories = _context.Categories
                    .Where(c => c.CategoryId > 0)
                    .OrderBy(c => c.Type)
                    .ThenBy(c => c.Title)
                    .AsEnumerable();

                ViewBag.Categories = categories;
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrEdit POST");
                ModelState.AddModelError("", "An error occurred while saving the transaction. Please try again.");
                
                var categories = _context.Categories
                    .Where(c => c.CategoryId > 0)
                    .OrderBy(c => c.Type)
                    .ThenBy(c => c.Title)
                    .AsEnumerable();

                ViewBag.Categories = categories;
                return View(transaction);
            }
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try 
            {
                if (_context.Transactions == null)
                {
                    return Problem("Entity set 'ApplicationDbContext.Transactions' is null.");
                }
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction != null)
                {
                    _context.Transactions.Remove(transaction);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "An error occurred while deleting the transaction: " + ex.Message
                });
            }
        }
    }
} 