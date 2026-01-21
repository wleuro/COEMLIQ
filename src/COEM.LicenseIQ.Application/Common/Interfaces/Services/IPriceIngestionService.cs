using COEM.LicenseIQ.Application.Common.Models;
using COEM.LicenseIQ.Domain.Entities; // Necesario para la lista de Country
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Services
{
    // Esta es la clase que faltaba:
    public class IngestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NewProducts { get; set; }
        public int UpdatedPrices { get; set; }
    }

    public interface IPriceIngestionService
    {
        Task<IngestResult> IngestPriceListAsync(Stream fileStream, int countryId, string countryIso, string listType, Guid userId, DateTime validityDate);

        Task<List<Country>> GetActiveCountriesAsync();

        //Task<List<ProductExplorerDto>> SearchProductsAsync(int countryId, DateTime validityMonth, string searchTerm);

        Task<bool> UpdateProductTaxCategoryAsync(string skuId, string newCategory);

        Task<List<ProductExplorerDto>> SearchProductsAsync(int countryId, DateTime validityMonth, string searchTerm, string segment = null);

        Task<List<string>> GetUniqueSegmentsAsync();

    }
}