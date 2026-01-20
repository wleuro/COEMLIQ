using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Domain.Entities;

public class TaxRule
{
    // Constructor privado para EF Core
    private TaxRule() { }

    // Constructor público para crear nuevas reglas
    public TaxRule(int countryId, ProductTaxCategory category, decimal rate, string legalReference)
    {
        if (rate < 0) throw new ArgumentException("La tasa no puede ser negativa.");

        CountryID = countryId;
        ProductTaxCategory = category;
        TaxRate = rate;
        LegalReference = legalReference;
        EffectiveDate = DateTime.UtcNow;
    }

    public int RuleID { get; private set; }
    public int CountryID { get; private set; }
    public ProductTaxCategory ProductTaxCategory { get; private set; }
    public decimal TaxRate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string LegalReference { get; private set; } = string.Empty;

    // === ESTE ES EL MÉTODO QUE FALTABA ===
    // Permite editar la tasa de manera controlada y auditada
    public void UpdateRate(decimal newRate, string newReference)
    {
        if (newRate < 0) throw new ArgumentException("La tasa no puede ser negativa.");

        TaxRate = newRate;

        // Si nos pasan una nueva referencia, la actualizamos. Si no, mantenemos la anterior o ponemos una genérica.
        if (!string.IsNullOrWhiteSpace(newReference))
        {
            LegalReference = newReference;
        }

        // Importante: Actualizamos la fecha para saber cuándo ocurrió el último cambio
        EffectiveDate = DateTime.UtcNow;
    }
}