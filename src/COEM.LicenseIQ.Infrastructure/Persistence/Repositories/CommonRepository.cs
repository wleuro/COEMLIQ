using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace COEM.LicenseIQ.Infrastructure.Persistence.Repositories;

public class CommonRepository : ICommonRepository
{
    private readonly ApplicationDbContext _context;

    public CommonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Country>> GetActiveCountriesAsync(CancellationToken cancellationToken)
    {
        // Retorna solo países marcados como IsActive = true, ordenados alfabéticamente
        return await _context.Countries
            .AsNoTracking() // Optimización: Solo lectura, no necesitamos trackear cambios
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Country?> GetCountryByIdAsync(int countryId, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .FirstOrDefaultAsync(c => c.CountryID == countryId, cancellationToken);
    }
}