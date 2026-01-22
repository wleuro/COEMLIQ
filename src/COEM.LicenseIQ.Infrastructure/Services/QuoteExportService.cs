using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ClosedXML.Excel;
using COEM.LicenseIQ.Domain.Entities.Quotes;

namespace COEM.LicenseIQ.Infrastructure.Services
{
    public class QuoteExportService
    {
        public byte[] GenerateExcel(Quote quote)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cotización");

            // --- 1. ESTILOS Y CABECERA ---
            // Logo o Nombre de Empresa
            worksheet.Cell("B2").Value = "CONTROLES EMPRESARIALES";
            worksheet.Cell("B2").Style.Font.Bold = true;
            worksheet.Cell("B2").Style.Font.FontSize = 16;
            worksheet.Cell("B2").Style.Font.FontColor = XLColor.FromHtml("#0056b3"); // Azul Corporativo

            // Datos del Cliente
            worksheet.Cell("B4").Value = "Cliente:";
            worksheet.Cell("C4").Value = quote.CustomerName;

            worksheet.Cell("B5").Value = "Proyecto:";
            worksheet.Cell("C5").Value = quote.ProjectName;

            worksheet.Cell("B6").Value = "Fecha:";
            worksheet.Cell("C6").Value = quote.CreatedDate.ToShortDateString();

            // --- 2. TABLA DE PRODUCTOS ---
            int currentRow = 9;

            // Encabezados de Tabla
            var headers = new[] { "SKU", "Producto", "Segmento", "Plazo", "Cantidad", "Precio Unit.", "Total" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 2); // Empezamos en columna B (2)
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f0f0");
                cell.Style.Font.Bold = true;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            currentRow++;

            // Filas de Items
            decimal grandTotal = 0;
            foreach (var item in quote.Items)
            {
                worksheet.Cell(currentRow, 2).Value = item.SkuId;
                worksheet.Cell(currentRow, 3).Value = item.ProductName;
                worksheet.Cell(currentRow, 4).Value = item.Segment;
                worksheet.Cell(currentRow, 5).Value = item.TermDuration + " / " + item.BillingPlan;

                worksheet.Cell(currentRow, 6).Value = item.Quantity;
                worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(currentRow, 7).Value = item.UnitPrice;
                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$ #,##0.00";

                worksheet.Cell(currentRow, 8).Value = item.TotalLine;
                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$ #,##0.00";
                worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

                grandTotal += item.TotalLine;
                currentRow++;
            }

            // --- 3. TOTALES ---
            currentRow += 2;
            worksheet.Cell(currentRow, 7).Value = "GRAN TOTAL:";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = grandTotal;
            worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$ #,##0.00";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#d4edda"); // Verde sutil

            // Ajuste automático de columnas
            worksheet.Columns().AdjustToContents();

            // Guardar en memoria
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}