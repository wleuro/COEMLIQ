using System;
using System.IO;
using ClosedXML.Excel;
using COEM.LicenseIQ.Domain.Entities.Quotes;
using System.Linq;

namespace COEM.LicenseIQ.Infrastructure.Services
{
    public class QuoteExportService
    {
        public byte[] GenerateExcel(Quote quote, string webRootPath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Oferta Económica");

            // --- 1. CONFIGURACIÓN VISUAL ---
            worksheet.ShowGridLines = false;

            // --- 2. LOGO (Variable renombrada a 'logoStream') ---
            string logoPath = Path.Combine(webRootPath, "images", "logo.png");

            if (File.Exists(logoPath))
            {
                try
                {
                    // CORRECCIÓN AQUÍ: Usamos 'logoStream' en lugar de 'stream'
                    using (var logoStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read))
                    {
                        var picture = worksheet.AddPicture(logoStream)
                                               .MoveTo(worksheet.Cell("B2"))
                                               .WithSize(200, 66); // 0.69" x 2.09" aprox
                    }
                }
                catch (Exception ex)
                {
                    worksheet.Cell("B2").Value = $"Error img: {ex.Message}";
                }
            }
            else
            {
                worksheet.Cell("B2").Value = "⚠️ Logo no encontrado";
                worksheet.Cell("B2").Style.Font.FontColor = XLColor.Red;
            }

            // --- 3. ENCABEZADO DE EMPRESA ---
            var titleCell = worksheet.Cell("B5");
            titleCell.Value = "CONTROLES EMPRESARIALES";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 14;
            titleCell.Style.Font.FontColor = XLColor.FromHtml("#FF0126");

            // Datos del Cliente
            worksheet.Cell("B7").Value = "Cliente:";
            worksheet.Cell("B7").Style.Font.Bold = true;
            worksheet.Cell("C7").Value = quote.CustomerName;

            worksheet.Cell("B8").Value = "Proyecto:";
            worksheet.Cell("B8").Style.Font.Bold = true;
            worksheet.Cell("C8").Value = quote.ProjectName;

            worksheet.Cell("B9").Value = "Fecha:";
            worksheet.Cell("B9").Style.Font.Bold = true;
            worksheet.Cell("C9").Value = quote.CreatedDate.ToShortDateString();

            worksheet.Cell("B10").Value = "Moneda:";
            worksheet.Cell("B10").Style.Font.Bold = true;
            worksheet.Cell("C10").Value = quote.Currency;

            // --- 4. TABLA DE PRODUCTOS ---
            int startRow = 12;
            int currentRow = startRow;

            var headers = new[] { "SKU", "Producto", "Tipo", "Cant.", "Precio Unit.", "Subtotal", "Impuesto", "Total" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 2);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            currentRow++;

            foreach (var item in quote.Items)
            {
                worksheet.Cell(currentRow, 2).Value = item.SkuId;
                worksheet.Cell(currentRow, 3).Value = item.ProductName;

                string tipo = (item.TaxCategory == "software_local") ? "Software" : "Nube";
                worksheet.Cell(currentRow, 4).Value = tipo;
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 5).Value = item.Quantity;
                worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 6).Value = item.UnitPrice;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(currentRow, 7).Value = item.SubTotal;
                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(currentRow, 8).Value = item.TaxAmount;
                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(currentRow, 9).Value = item.TotalLine;
                worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

                currentRow++;
            }

            // --- 5. BORDES Y ESTILOS ---
            var tableRange = worksheet.Range(startRow, 2, currentRow - 1, 9);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.OutsideBorderColor = XLColor.Gray;
            tableRange.Style.Border.InsideBorderColor = XLColor.LightGray;

            // --- 6. TOTALES ---
            currentRow += 1;

            // Subtotal
            worksheet.Cell(currentRow, 8).Value = "Subtotal:";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(currentRow, 9).Value = quote.Items.Sum(i => i.SubTotal);
            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "$ #,##0.00";

            currentRow++;
            // Impuestos
            worksheet.Cell(currentRow, 8).Value = "Impuestos:";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(currentRow, 9).Value = quote.Items.Sum(i => i.TaxAmount);
            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "$ #,##0.00";
            worksheet.Cell(currentRow, 9).Style.Font.FontColor = XLColor.Red;

            currentRow++;
            // Gran Total
            worksheet.Cell(currentRow, 8).Value = "TOTAL:";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(currentRow, 9).Value = quote.Items.Sum(i => i.TotalLine);
            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "$ #,##0.00";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#E6E6E6");

            worksheet.Range(currentRow - 2, 8, currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Ajuste automático
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 40;
            worksheet.Columns(4, 9).AdjustToContents();

            // CORRECCIÓN AQUÍ: Usamos 'memoryStream' para evitar colisiones
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }
    }
}