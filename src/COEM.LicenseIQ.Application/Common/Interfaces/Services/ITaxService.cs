using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Models;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Services;

public interface ITaxService
{
    Task<TaxCalculationResult> CalculateTaxAsync(
        int originCountryId,
        int destCountryId,
        ProductTaxCategory category,
        ClientTaxProfile clientProfile,
        decimal baseAmount, // Precio sobre el cual calcular
        CancellationToken cancellationToken);
}