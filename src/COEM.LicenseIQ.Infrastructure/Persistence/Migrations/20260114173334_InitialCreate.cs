using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COEM.LicenseIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRulesEngine",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginCountryID = table.Column<int>(type: "int", nullable: false),
                    DestCountryID = table.Column<int>(type: "int", nullable: false),
                    ProductTaxCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientTaxProfile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LegalReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRulesEngine", x => x.RuleID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_SearchVector",
                table: "TaxRulesEngine",
                columns: new[] { "OriginCountryID", "DestCountryID", "ProductTaxCategory", "ClientTaxProfile" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRulesEngine");
        }
    }
}
