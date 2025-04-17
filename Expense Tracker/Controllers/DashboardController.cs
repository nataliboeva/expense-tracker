using Expense_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Default to last 7 days if no dates provided
            DateTime StartDate = startDate ?? DateTime.Today.AddDays(-6);
            DateTime EndDate = endDate ?? DateTime.Today;

            var selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(x => x.Date >= StartDate && x.Date <= EndDate)
                .ToListAsync();

            // Total Income
            decimal TotalIncome = selectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C2");

            // Total Expense
            decimal TotalExpense = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C2");

            // Balance
            decimal Balance = TotalIncome - TotalExpense;
            ViewBag.Balance = Balance.ToString("C2");

            // Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.Title)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C2"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            // Spline Chart - Income vs Expense

            // Get all dates in range
            int totalDays = (EndDate - StartDate).Days + 1;
            string[] DateRange = Enumerable.Range(0, totalDays)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            // Income
            List<SplineChartData> IncomeSummary = selectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            // Expense
            List<SplineChartData> ExpenseSummary = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            // Combine Income & Expense
            ViewBag.SplineChartData = from day in DateRange
                                    join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                    from income in dayIncomeJoined.DefaultIfEmpty()
                                    join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                    from expense in expenseJoined.DefaultIfEmpty()
                                    select new
                                    {
                                        day = day,
                                        income = income == null ? 0m : income.income,
                                        expense = expense == null ? 0m : expense.expense,
                                    };

            // Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            // Pass selected date range back to view
            ViewBag.StartDate = StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = EndDate.ToString("yyyy-MM-dd");

            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public decimal income;
        public decimal expense;
    }
} 