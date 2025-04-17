namespace Expense_Tracker.Models
{
    using Microsoft.EntityFrameworkCore;
    using static DbConfiguration;
    public partial class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .Property(c => c.Icon)
                .HasColumnType("nvarchar(100)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Currency)
                .HasDefaultValue("EUR");

            // Seed initial categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Title = "Salary", Icon = "bi bi-currency-dollar", Type = "Income" },
                new Category { CategoryId = 2, Title = "Food & Dining", Icon = "bi bi-cup-hot-fill", Type = "Expense" },
                new Category { CategoryId = 3, Title = "Shopping", Icon = "bi bi-cart-fill", Type = "Expense" },
                new Category { CategoryId = 4, Title = "Freelance", Icon = "bi bi-laptop-fill", Type = "Income" },
                new Category { CategoryId = 5, Title = "Bills", Icon = "bi bi-receipt", Type = "Expense" }
            );
        }
    }
}
