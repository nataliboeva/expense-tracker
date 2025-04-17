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

namespace Expense_Tracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string DefaultCurrency = "EUR";

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
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
                                // Log error but don't break the page
                                Console.WriteLine($"Currency conversion error: {ex.Message}");
                            }
                        }
                    }
                }

                return View(transactions);
            }
            catch (Exception ex)
            {
                // Log the exception and return a friendly error view
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        // GET: Transaction/AddOrEdit
        public IActionResult AddOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Transaction { Currency = DefaultCurrency });
            else
                return View(_context.Transactions.Find(id));
        }

        // POST: Transaction/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date,Currency")] Transaction transaction)
        {
            // Normalize DOLLAR to USD
            if (transaction.Currency == "DOLLAR")
            {
                transaction.Currency = "USD";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (transaction.TransactionId == 0)
                        _context.Add(transaction);
                    else
                    {
                        var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
                        if (existingTransaction.Currency != transaction.Currency)
                        {
                            // Convert amount from old currency to new currency
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
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error in AddOrEdit: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the transaction. Please try again.");
                }
            }

            PopulateCategories();
            return View(transaction);
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
                // Log the exception
                Console.WriteLine($"Error in Delete: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while deleting the transaction.";
                return View("Error");
            }
        }

        [NonAction]
        public void PopulateCategories()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }
    }
} 