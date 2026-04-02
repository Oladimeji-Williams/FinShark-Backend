using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinShark.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseSymbolLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "Stocks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "Stock ticker symbol (e.g., AAPL, MSFT)",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldComment: "Stock ticker symbol (e.g., AAPL, MSFT)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "Stocks",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                comment: "Stock ticker symbol (e.g., AAPL, MSFT)",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "Stock ticker symbol (e.g., AAPL, MSFT)");
        }
    }
}
