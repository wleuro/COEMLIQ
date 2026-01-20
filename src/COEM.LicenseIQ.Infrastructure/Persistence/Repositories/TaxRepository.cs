using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;
using COEM.LicenseIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace COEM.LicenseIQ.Infrastructure.Persistence.Repositories;

public class TaxRepository : ITaxRepository
{
    private readonly ApplicationDbContext _context;

    public TaxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateAsync(TaxRule rule, CancellationToken cancellationToken)
    {
        // EF Core detecta los cambios automáticamente si la entidad está trackeada,
        // pero forzamos el Update para ser explícitos.
        _context.TaxRules.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int ruleId, CancellationToken cancellationToken)
    {
        // 1. Buscamos la entidad por ID
        var entity = await _context.TaxRules.FindAsync(new object[] { ruleId }, cancellationToken);

        // 2. Si existe, la borramos
        if (entity != null)
        {
            _context.TaxRules.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
    public async Task AddAsync(TaxRule rule, CancellationToken cancellationToken)
    {
        await _context.TaxRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaxRule?> GetByIdAsync(int ruleId, CancellationToken cancellationToken)
    {
        return await _context.TaxRules.FindAsync(new object[] { ruleId }, cancellationToken);
    }

    public async Task<List<TaxRule>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.TaxRules.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<TaxRule?> GetMatchAsync(
    int countryId,
    ProductTaxCategory category,
    CancellationToken cancellationToken)
    {
        return await _context.TaxRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.CountryID == countryId &&
                r.ProductTaxCategory == category,
                cancellationToken);
    }
}