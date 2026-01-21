using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COEM.LicenseIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCSPAndMarket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CSP_Products",
                columns: table => new
                {
                    SkuId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SkuTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProductTaxCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsManualOverride = table.Column<bool>(type: "bit", nullable: false),
                    Segment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CSP_Products", x => x.SkuId);
                });

            migrationBuilder.CreateTable(
                name: "PriceListImports",
                columns: table => new
                {
                    ImportID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    ListType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListImports", x => x.ImportID);
                });

            migrationBuilder.CreateTable(
                name: "CSP_PriceList",
                columns: table => new
                {
                    PriceID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkuId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    ImportID = table.Column<long>(type: "bigint", nullable: false),
                    Market = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistoryJSON = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CSP_PriceList", x => x.PriceID);
                    table.ForeignKey(
                        name: "FK_CSP_PriceList_CSP_Products_SkuId",
                        column: x => x.SkuId,
                        principalTable: "CSP_Products",
                        principalColumn: "SkuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CSP_PriceList_Countries_CountryID",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CSP_PriceList_CountryID",
                table: "CSP_PriceList",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_CSP_PriceList_SkuId",
                table: "CSP_PriceList",
                column: "SkuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CSP_PriceList");

            migrationBuilder.DropTable(
                name: "PriceListImports");

            migrationBuilder.DropTable(
                name: "CSP_Products");

        }
    }
}
