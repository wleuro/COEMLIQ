using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using COEM.LicenseIQ.Domain.Entities; // Necesario para la lista de Country

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
    }
}