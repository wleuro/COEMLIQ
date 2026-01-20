using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Application.Common.Models;
using COEM.LicenseIQ.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace COEM.LicenseIQ.Application.Services;

public class TaxService : ITaxService
{
    private readonly ITaxRepository _taxRepository;
    private readonly ILogger<TaxService> _logger;

    public TaxService(ITaxRepository taxRepository, ILogger<TaxService> logger)
    {
        _taxRepository = taxRepository;
        _logger = logger;
    }

    public async Task<TaxCalculationResult> CalculateTaxAsync(
        int countryId,
        ProductTaxCategory category,
        decimal baseAmount,
        CancellationToken cancellationToken)
    {
        // 1. Consultar la Matriz de Reglas
        // Buscamos: "¿Cuál es la tasa para [Producto] en [País]?"
        var rule = await _taxRepository.GetMatchAsync(countryId, category, cancellationToken);

        // 2. Escenario Happy Path (Regla encontrada en base de datos)
        if (rule != null)
        {
            return TaxCalculationResult.Success(
                rule.TaxRate,
                baseAmount,
                rule.LegalReference,
                rule.RuleID);
        }

        // 3. Escenario Anomalía (Falta configuración)
        // Esto ocurre si abres un país nuevo (ej. Chile) y olvidaste configurar sus tasas.
        _logger.LogWarning("Tax Anomaly: No rule found for CountryID:{Country} Category:{Cat}",
            countryId, category);

        // 4. Fallback Defensivo (Prudencia Financiera)
        // Ante la duda, aplicamos una tasa estándar (19%) para proteger el margen.
        // Es preferible que sobre dinero (devolución) a que falte en la auditoría.
        decimal fallbackRate = 19.00m;

        return TaxCalculationResult.Default(
            fallbackRate,
            baseAmount,
            "FALLBACK: Regla no configurada. Se aplica tasa estándar de protección.");
    }
}