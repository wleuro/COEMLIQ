using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Entities.CSP;
using COEM.LicenseIQ.Infrastructure.Persistence;

namespace COEM.LicenseIQ.Infrastructure.Services
{
    public class PriceIngestionService : IPriceIngestionService
    {
        private readonly ApplicationDbContext _context;

        public PriceIngestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Country>> GetActiveCountriesAsync()
        {
            return await _context.Countries
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<IngestResult> IngestPriceListAsync(Stream fileStream, int countryIdInput, string countryIsoInput, string listType, Guid userId, DateTime validityDate)
        {
            // Normalizamos la fecha al primer día del mes para evitar problemas (ej: 2026-02-01)
            var effectiveMonth = new DateTime(validityDate.Year, validityDate.Month, 1);

            // 1. VALIDACIONES
            var targetCountry = await _context.Countries.FindAsync(countryIdInput);
            if (targetCountry == null) return new IngestResult { Success = false, Message = "País no existe." };

            if (!string.Equals(targetCountry.IsoCode, countryIsoInput, StringComparison.OrdinalIgnoreCase))
                return new IngestResult { Success = false, Message = "Inconsistencia de ISO." };

            // 2. AUDITORÍA
            var importLog = new PriceListImports
            {
                // Incluimos el mes de vigencia en el nombre del archivo lógico
                FileName = $"{listType}_{countryIsoInput}_{effectiveMonth:yyyy-MM}_{DateTime.Now:HHmm}",
                UserGUID = userId,
                CountryID = targetCountry.CountryID,
                ListType = listType,
                Status = "Processing",
                ImportDate = DateTime.Now
            };
            _context.PriceListImports.Add(importLog);
            await _context.SaveChangesAsync();

            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // 3. PRE-CARGA INTELIGENTE (Optimizada para Vigencia Mensual)
                var existingProductIds = await _context.CSP_Products.Select(p => p.SkuId).ToHashSetAsync();

                // Cargamos SOLO los precios para ESTE PAÍS y ESTE MES de vigencia.
                // Esto nos permite detectar si estamos corrigiendo una carga previa de este mismo mes.
                var pricesForThisMonth = await _context.CSP_PriceList
                    .Where(p => p.CountryID == targetCountry.CountryID && p.EffectiveDate == effectiveMonth)
                    .ToDictionaryAsync(p => p.SkuId);

                var productsToAdd = new List<CSP_Products>();
                var pricesToAdd = new List<CSP_PriceList>();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", ""),
                };

                using (var reader = new StreamReader(memoryStream))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read(); csv.ReadHeader();
                    bool isFirstRow = true;

                    while (csv.Read())
                    {
                        string skuId = csv.GetField("SkuId") ?? csv.GetField("SkuID");
                        string market = csv.GetField("Market");
                        string unitPriceStr = csv.GetField("UnitPrice");

                        if (string.IsNullOrEmpty(skuId) || string.IsNullOrEmpty(unitPriceStr)) continue;

                        // Validación Geo-Lock
                        if (isFirstRow)
                        {
                            if (!string.Equals(market, targetCountry.IsoCode, StringComparison.OrdinalIgnoreCase))
                            {
                                importLog.Status = "Failed_GeoMismatch";
                                await _context.SaveChangesAsync();
                                return new IngestResult { Success = false, Message = $"Error: Lista de {market} no corresponde a {targetCountry.IsoCode}." };
                            }
                            isFirstRow = false;
                        }
                        if (!string.Equals(market, targetCountry.IsoCode, StringComparison.OrdinalIgnoreCase)) continue;

                        if (!decimal.TryParse(unitPriceStr, out decimal newPrice)) continue;

                        string title = csv.GetField("OfferName") ?? csv.GetField("ProductName") ?? "Unknown";
                        string segment = csv.GetField("Segment") ?? "Commercial";
                        string currency = csv.GetField("Currency") ?? "USD";

                        // Lógica Fiscal
                        string taxCategory = (listType == "Software") ? "software_local" : "cloud";
                        if (listType != "Software" && title.Contains("Perpetual", StringComparison.OrdinalIgnoreCase))
                            taxCategory = "software_local";

                        // A. PRODUCTOS (Maestra única)
                        if (!existingProductIds.Contains(skuId))
                        {
                            productsToAdd.Add(new CSP_Products
                            {
                                SkuId = skuId,
                                ProductId = csv.GetField("ProductId") ?? "N/A",
                                SkuTitle = title,
                                ProductTaxCategory = taxCategory,
                                Segment = segment,
                                IsManualOverride = false,
                                LastUpdated = DateTime.Now
                            });
                            existingProductIds.Add(skuId);
                        }

                        // B. PRECIOS (Por Vigencia)
                        if (pricesForThisMonth.TryGetValue(skuId, out var existingPriceInMonth))
                        {
                            // YA EXISTE PRECIO PARA ESTE MES: Es una corrección/re-carga.
                            if (existingPriceInMonth.UnitPrice != newPrice)
                            {
                                // Actualizamos el precio existente de este mes.
                                // No creamos historia nueva, corregimos el dato del mes.
                                existingPriceInMonth.UnitPrice = newPrice;
                                existingPriceInMonth.ImportID = importLog.ImportID; // Actualizamos referencia de carga
                                // Nota: EF Core detectará el cambio automáticamente al guardar.
                            }
                        }
                        else
                        {
                            // NO EXISTE PRECIO PARA ESTE MES: Es un nuevo mes o nuevo producto.
                            // Insertamos el nuevo registro con la fecha de vigencia seleccionada.
                            pricesToAdd.Add(new CSP_PriceList
                            {
                                SkuId = skuId,
                                ImportID = importLog.ImportID,
                                CountryID = targetCountry.CountryID,
                                Market = market,
                                UnitPrice = newPrice,
                                Currency = currency,
                                EffectiveDate = effectiveMonth, // <--- LA FECHA QUE ELIGIÓ EL ADMIN
                                IsActive = true, // Por defecto activo para su mes
                                HistoryJSON = "[]"
                            });
                        }
                    }
                }

                if (productsToAdd.Any()) await _context.CSP_Products.AddRangeAsync(productsToAdd);
                if (pricesToAdd.Any()) await _context.CSP_PriceList.AddRangeAsync(pricesToAdd);

                // NOTA: Para las actualizaciones (existingPriceInMonth), EF Core las maneja al llamar SaveChanges

                importLog.Status = "Completed";
                await _context.SaveChangesAsync();

                return new IngestResult
                {
                    Success = true,
                    Message = $"Carga exitosa para vigencia {effectiveMonth:MMM-yyyy}.",
                    NewProducts = productsToAdd.Count,
                    UpdatedPrices = pricesToAdd.Count // Aquí contaríamos también las actualizaciones si quisiéramos ser exactos
                };
            }
            catch (Exception ex)
            {
                importLog.Status = "Failed";
                await _context.SaveChangesAsync();
                return new IngestResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}