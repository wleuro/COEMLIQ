using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Entities.CSP;
using COEM.LicenseIQ.Infrastructure.Persistence;
using COEM.LicenseIQ.Application.Common.Models;

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

        public async Task<List<ProductExplorerDto>> SearchProductsAsync(int countryId, DateTime validityMonth, string searchTerm, string segment = null)
        {
            var targetMonth = new DateTime(validityMonth.Year, validityMonth.Month, 1);
            var term = searchTerm?.ToLower() ?? "";

            var query = from price in _context.CSP_PriceList
                        join prod in _context.CSP_Products on price.SkuId equals prod.SkuId
                        where price.CountryID == countryId
                           && price.EffectiveDate.Year == targetMonth.Year
                           && price.EffectiveDate.Month == targetMonth.Month
                           && price.IsActive
                        select new { price, prod };

            // Filtro Texto
            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x => x.prod.SkuTitle.ToLower().Contains(term) || x.prod.SkuId.ToLower().Contains(term));
            }

            // NUEVO: Filtro Segmento
            if (!string.IsNullOrWhiteSpace(segment) && segment != "All")
            {
                query = query.Where(x => x.prod.Segment == segment);
            }

            // Aumentamos el límite a 500 ahora que tenemos más filtros
            var results = await query.OrderBy(x => x.prod.SkuTitle)
                                     .Take(500)
                                     .Select(x => new ProductExplorerDto
                                     {
                                         SkuId = x.prod.SkuId,
                                         ProductName = x.prod.SkuTitle,
                                         Segment = x.prod.Segment,
                                         UnitPrice = x.price.UnitPrice,
                                         Currency = x.price.Currency,
                                         TaxCategory = x.prod.ProductTaxCategory,
                                         IsManualOverride = x.prod.IsManualOverride,
                                         LastUpdated = x.prod.LastUpdated
                                     }).ToListAsync();

            return results;
        }

        // Nuevo método para llenar el dropdown
        public async Task<List<string>> GetUniqueSegmentsAsync()
        {
            return await _context.CSP_Products
                .Select(p => p.Segment)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<bool> UpdateProductTaxCategoryAsync(string skuId, string newCategory)
        {
            if (string.IsNullOrWhiteSpace(skuId)) return false;

            try
            {
                // 1. Limpieza del ID por seguridad
                var cleanId = skuId.Trim();

                // 2. Búsqueda directa (sin caché de FindAsync para evitar datos rancios)
                var product = await _context.CSP_Products
                    .FirstOrDefaultAsync(p => p.SkuId == cleanId);

                if (product == null) return false;

                // 3. Aplicar cambios
                product.ProductTaxCategory = newCategory;
                product.IsManualOverride = true;
                product.LastUpdated = DateTime.Now;

                // 4. FUERZA BRUTA: Le gritamos a EF Core que esto cambió
                _context.Entry(product).State = EntityState.Modified;

                // 5. Guardar
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loguear error si tienes logger, por ahora retornamos false
                Console.WriteLine($"Error actualizando categoría: {ex.Message}");
                return false;
            }
        }

        public async Task<IngestResult> IngestPriceListAsync(Stream fileStream, int countryIdInput, string countryIsoInput, string listType, Guid userId, DateTime validityDate)
        {
            var effectiveMonth = new DateTime(validityDate.Year, validityDate.Month, 1);
            var targetCountry = await _context.Countries.FindAsync(countryIdInput);

            if (targetCountry == null || !string.Equals(targetCountry.IsoCode, countryIsoInput, StringComparison.OrdinalIgnoreCase))
                return new IngestResult { Success = false, Message = "Error de validación de País." };

            var importLog = new PriceListImports
            {
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

                // 1. CARGA DE REFERENCIAS (HashSet/Dict para velocidad)
                var existingProductIds = await _context.CSP_Products.Select(p => p.SkuId).ToHashSetAsync();

                // Diccionario de precios existentes para este mes
                var pricesForThisMonth = await _context.CSP_PriceList
                    .Where(p => p.CountryID == targetCountry.CountryID && p.EffectiveDate == effectiveMonth)
                    .ToDictionaryAsync(p => p.SkuId);

                var productsToAdd = new List<CSP_Products>();
                var pricesToAdd = new List<CSP_PriceList>();

                using (var reader = new StreamReader(memoryStream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { MissingFieldFound = null, HeaderValidated = null }))
                {
                    // LEEMOS COMO "DYNAMIC" PARA NO DEPENDER DE NOMBRES DE COLUMNAS FIJOS
                    var records = csv.GetRecords<dynamic>();

                    foreach (var row in records)
                    {
                        // Convertimos la fila dinámica a un Diccionario para manipularla fácil
                        var rowDict = (IDictionary<string, object>)row;

                        // BÚSQUEDA TOLERANTE DE CAMPOS CLAVE
                        // Buscamos las llaves ignorando mayúsculas/espacios
                        string GetValue(string keyPart) =>
                            rowDict.FirstOrDefault(k => k.Key.Replace(" ", "").Contains(keyPart, StringComparison.OrdinalIgnoreCase)).Value?.ToString();

                        string skuId = GetValue("SkuId") ?? GetValue("SkuID") ?? GetValue("PartNumber") ?? GetValue("Material");
                        string unitPriceStr = GetValue("UnitPrice");
                        string market = GetValue("Market");

                        if (string.IsNullOrEmpty(skuId) || string.IsNullOrEmpty(unitPriceStr)) continue;

                        // Geo-Lock
                        if (!string.IsNullOrEmpty(market) && !market.Equals(targetCountry.IsoCode, StringComparison.OrdinalIgnoreCase)) continue;

                        if (!decimal.TryParse(unitPriceStr, out decimal newPrice)) continue;

                        string title = GetValue("OfferName")
                                    ?? GetValue("ProductName")
                                    ?? GetValue("Name")
                                    ?? GetValue("Title")
                                    ?? GetValue("Description")
                                    ?? GetValue("SkuTitle")
                                    ?? "Unknown";
                        string currency = GetValue("Currency") ?? "USD";

                        // --- AQUÍ OCURRE LA MAGIA DEL JSON ---
                        // Serializamos TODO el diccionario de la fila. Si hay 50 columnas extrañas, se guardan aquí.
                        string rawJson = JsonSerializer.Serialize(rowDict);

                        // Lógica Fiscal (Simplificada)
                        string taxCategory = (listType == "Software" || title.Contains("Perpetual", StringComparison.OrdinalIgnoreCase))
                                             ? "software_local" : "cloud";

                        // A. PRODUCTO (Upsert Lógico)
                        if (!existingProductIds.Contains(skuId))
                        {
                            productsToAdd.Add(new CSP_Products
                            {
                                SkuId = skuId,
                                ProductId = GetValue("ProductId") ?? "N/A",
                                SkuTitle = title,
                                ProductTaxCategory = taxCategory,
                                Segment = GetValue("Segment") ?? "Commercial",
                                LastUpdated = DateTime.Now
                            });
                            existingProductIds.Add(skuId);
                        }

                        // B. PRECIO (Con RawData)
                        if (pricesForThisMonth.TryGetValue(skuId, out var existingPrice))
                        {
                            if (existingPrice.UnitPrice != newPrice)
                            {
                                existingPrice.UnitPrice = newPrice;
                                existingPrice.RawData = rawJson; // Actualizamos la data cruda también
                                existingPrice.ImportID = importLog.ImportID;
                            }
                        }
                        else
                        {
                            pricesToAdd.Add(new CSP_PriceList
                            {
                                SkuId = skuId,
                                ImportID = importLog.ImportID,
                                CountryID = targetCountry.CountryID,
                                // Market ya no es obligatorio en la tabla si tenemos RawData, pero lo dejamos por consistencia
                                Market = targetCountry.IsoCode,
                                UnitPrice = newPrice,
                                Currency = currency,
                                EffectiveDate = effectiveMonth,
                                IsActive = true,
                                RawData = rawJson // <--- AQUÍ SE GUARDA LA EVIDENCIA COMPLETA
                            });
                        }
                    }
                }

                if (productsToAdd.Any()) await _context.CSP_Products.AddRangeAsync(productsToAdd);
                if (pricesToAdd.Any()) await _context.CSP_PriceList.AddRangeAsync(pricesToAdd);

                importLog.Status = "Completed";
                await _context.SaveChangesAsync();

                return new IngestResult { Success = true, Message = "Carga flexible completada.", NewProducts = productsToAdd.Count, UpdatedPrices = pricesToAdd.Count };
            }
            catch (Exception ex)
            {
                importLog.Status = "Failed";
                importLog.FileName += "_ERR"; // Marcamos error en nombre para debug visual
                await _context.SaveChangesAsync();
                return new IngestResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

    }
}