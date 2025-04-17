using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker.Migrations
{
    public partial class AddCurrencyToTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Transactions",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "USD");

            // Update existing records to have USD as currency
            migrationBuilder.Sql("UPDATE Transactions SET Currency = 'USD' WHERE Currency IS NULL OR Currency = ''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Transactions");
        }
    }
} 