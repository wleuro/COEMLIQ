using System;
using System.Collections.Generic;
using System.Text;

namespace COEM.LicenseIQ.Application.Common.Models;

public record TaxCalculationResult
{
    public decimal Rate { get; init; }
    public decimal TaxAmount { get; init; }
    public bool IsExactMatch { get; init; } // True: Encontró regla específica. False: Usó Default.
    public string AppliedRuleReference { get; init; } = string.Empty; // Ej: "Ley 2010 - Art 4" o "Default Country Rate"
    public int? RuleId { get; init; } // Null si fue un fallback hardcoded

    // Factory method para facilitar la creación de resultados exitosos
    public static TaxCalculationResult Success(decimal rate, decimal baseAmount, string reference, int ruleId)
    {
        return new TaxCalculationResult
        {
            Rate = rate,
            TaxAmount = baseAmount * (rate / 100m), // Asumiendo rate viene como 19.00
            IsExactMatch = true,
            AppliedRuleReference = reference,
            RuleId = ruleId
        };
    }

    // Factory method para fallbacks (cuando no hay regla)
    public static TaxCalculationResult Default(decimal defaultRate, decimal baseAmount, string reason)
    {
        return new TaxCalculationResult
        {
            Rate = defaultRate,
            TaxAmount = baseAmount * (defaultRate / 100m),
            IsExactMatch = false, // Bandera roja para auditoría
            AppliedRuleReference = reason,
            RuleId = null
        };
    }
}