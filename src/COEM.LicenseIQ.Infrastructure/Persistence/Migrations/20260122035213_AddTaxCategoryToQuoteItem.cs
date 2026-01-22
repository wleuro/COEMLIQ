using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COEM.LicenseIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxCategoryToQuoteItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "QuoteItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxCategory",
                table: "QuoteItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "QuoteItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "TaxCategory",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "QuoteItems");
        }
    }
}
