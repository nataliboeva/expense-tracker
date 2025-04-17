using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker.Migrations
{
    public partial class FixIconColumnLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing tables
            migrationBuilder.DropTable(name: "Transactions");
            migrationBuilder.DropTable(name: "Categories");

            // Recreate Categories table with correct column length
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            // Recreate Transactions table
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(75)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", nullable: false, defaultValue: "EUR")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            // Seed initial categories with new icon format
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Title", "Icon", "Type" },
                values: new object[,]
                {
                    { 1, "Salary", "bi bi-currency-dollar", "Income" },
                    { 2, "Food & Dining", "bi bi-cup-hot-fill", "Expense" },
                    { 3, "Shopping", "bi bi-cart-fill", "Expense" },
                    { 4, "Freelance", "bi bi-laptop-fill", "Income" },
                    { 5, "Bills", "bi bi-receipt", "Expense" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Transactions");
            migrationBuilder.DropTable(name: "Categories");
        }
    }
} 