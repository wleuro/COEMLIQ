using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Persistence;

public interface ICommonRepository
{
    Task<List<Country>> GetActiveCountriesAsync(CancellationToken cancellationToken);

    Task<Country?> GetCountryByIdAsync(int countryId, CancellationToken cancellationToken);
}