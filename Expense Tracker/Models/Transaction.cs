using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(10)")]
        [RegularExpression("^(EUR|BGN|USD|DOLLAR)$", ErrorMessage = "Supported currencies are EUR, BGN, and USD")]
        public string Currency { get; set; } = "EUR";
    }
}
