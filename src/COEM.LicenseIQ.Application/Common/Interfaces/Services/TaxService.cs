using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Application.Common.Models;
using COEM.LicenseIQ.Domain.Enums;
using Microsoft.Extensions.Logging; // Indispensable para monitoreo

namespace COEM.LicenseIQ.Application.Services;

public class TaxService : ITaxService
{
    private readonly ITaxRepository _taxRepository;
    private readonly ILogger<TaxService> _logger;

    // Inyección de dependencias: Pedimos el Repo y el Logger
    public TaxService(ITaxRepository taxRepository, ILogger<TaxService> logger)
    {
        _taxRepository = taxRepository;
        _logger = logger;
    }

    public async Task<TaxCalculationResult> CalculateTaxAsync(
        int originCountryId,
        int destCountryId,
        ProductTaxCategory category,
        ClientTaxProfile clientProfile,
        decimal baseAmount,
        CancellationToken cancellationToken)
    {
        // 1. Consultar el Motor de Reglas (La Matriz)
        var rule = await _taxRepository.GetMatchAsync(originCountryId, destCountryId, category, clientProfile, cancellationToken);

        // 2. Escenario A: Coincidencia Exacta (Happy Path)
        if (rule != null)
        {
            return TaxCalculationResult.Success(
                rule.TaxRate,
                baseAmount,
                rule.LegalReference,
                rule.RuleID);
        }

        // 3. Escenario B: No hay regla (Anomalía)
        // Aquí aplicamos la lógica de "Resguardo" definida en el documento.
        // ADVERTENCIA: En un sistema Zenith, esto genera ruido en los logs.

        _logger.LogWarning("Tax Anomaly Detected: No rule found for Origin:{Origin} Dest:{Dest} Cat:{Cat} Profile:{Profile}",
            originCountryId, destCountryId, category, clientProfile);

        // Lógica de Fallback (Simplificada para MVP, debería venir de config o DB de países)
        // Por seguridad fiscal, ante la duda, aplicamos la tasa estándar local si es consumo local.
        decimal fallbackRate = (originCountryId == destCountryId) ? 19.00m : 0.00m;

        // OJO Will: Este fallback hardcoded es peligroso. 
        // Idealmente deberíamos tener un método _countryRepository.GetDefaultRate(destCountryId).
        // Pero para avanzar, usamos esto y marcamos IsExactMatch = false.

        return TaxCalculationResult.Default(
            fallbackRate,
            baseAmount,
            "FALLBACK: No explicit rule found. Applied system default.");
    }
}