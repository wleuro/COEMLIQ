using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Persistence;

public interface ITaxRepository
{
    /// <summary>
    /// Busca una regla fiscal exacta basada en los 4 vectores de decisión.
    /// Retorna null si no existe una regla explícita (lo que disparará la lógica de "Default" o "Revisión").
    /// </summary>
    Task<TaxRule?> GetMatchAsync(
        int originCountryId,
        int destCountryId,
        ProductTaxCategory category,
        ClientTaxProfile clientProfile,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene una regla por su ID para auditoría o edición.
    /// </summary>
    Task<TaxRule?> GetByIdAsync(int ruleId, CancellationToken cancellationToken);

    /// <summary>
    /// Persiste una nueva regla fiscal (para el panel de administración).
    /// </summary>
    Task AddAsync(TaxRule rule, CancellationToken cancellationToken);

    // NOTA: No necesitamos 'UpdateAsync' explícito si usamos EF Core y el patrón UnitOfWork,
    // ya que EF rastrea los cambios en la entidad cargada. Pero eso es problema de Infrastructure.
}