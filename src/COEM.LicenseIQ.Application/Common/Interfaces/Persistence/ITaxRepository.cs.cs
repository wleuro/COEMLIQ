using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Persistence;

public interface ITaxRepository
{
    Task UpdateAsync(TaxRule rule, CancellationToken cancellationToken);
    Task DeleteAsync(int ruleId, CancellationToken cancellationToken);

    Task<TaxRule?> GetMatchAsync(int countryId, ProductTaxCategory category, CancellationToken cancellationToken);
    Task<TaxRule?> GetByIdAsync(int ruleId, CancellationToken cancellationToken);

    Task AddAsync(TaxRule rule, CancellationToken cancellationToken);

    // Método extra para tu Panel Administrativo (opcional pero recomendado)
    Task<List<TaxRule>> GetAllAsync(CancellationToken cancellationToken);
}