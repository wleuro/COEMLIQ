using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Models;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Services;

public interface ITaxService
{
    /// <summary>
    /// Calcula el impuesto basado en la jurisdicción (País) y el tipo de producto.
    /// Simplificado: Ya no requiere origen/destino, solo el país de la transacción.
    /// </summary>
    Task<TaxCalculationResult> CalculateTaxAsync(
        int countryId,
        ProductTaxCategory category,
        decimal baseAmount,
        CancellationToken cancellationToken);
}