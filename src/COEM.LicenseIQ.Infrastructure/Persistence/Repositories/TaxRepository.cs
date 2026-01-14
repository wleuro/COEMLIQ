using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace COEM.LicenseIQ.Infrastructure.Persistence.Repositories;

public class TaxRepository : ITaxRepository
{
    private readonly ApplicationDbContext _context;

    public TaxRepository(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task<TaxRule?> GetMatchAsync(
        int originCountryId,
        int destCountryId,
        ProductTaxCategory category,
        ClientTaxProfile clientProfile,
        CancellationToken cancellationToken)
    {
        // Esta consulta usará el Índice "IX_TaxRules_SearchVector" que definimos.
        // Será increíblemente rápida incluso con millones de reglas.
        return await _context.TaxRules
            .AsNoTracking() // Optimización: No necesitamos trackear cambios para leer una tasa
            .FirstOrDefaultAsync(r =>
                r.OriginCountryID == originCountryId &&
                r.DestCountryID == destCountryId &&
                r.ProductTaxCategory == category &&
                r.ClientTaxProfile == clientProfile,
                cancellationToken);
    }
}