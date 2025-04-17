namespace Expense_Tracker.Models
{
    using Microsoft.EntityFrameworkCore;
    using static DbConfiguration;
    public partial class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Currency)
                .HasDefaultValue("EUR");

            // Seed initial categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Title = "Salary", Icon = "💰", Type = "Income" },
                new Category { CategoryId = 2, Title = "Food & Dining", Icon = "🍽️", Type = "Expense" },
                new Category { CategoryId = 3, Title = "Shopping", Icon = "🛍️", Type = "Expense" },
                new Category { CategoryId = 4, Title = "Freelance", Icon = "💻", Type = "Income" },
                new Category { CategoryId = 5, Title = "Bills", Icon = "📄", Type = "Expense" }
            );
        }
    }
}
