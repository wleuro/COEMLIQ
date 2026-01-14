using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Enums;

namespace COEM.LicenseIQ.Domain.Entities;

public class TaxRule
{
    // Constructor privado para EF Core (requerido para materialización)
    private TaxRule() { }

    // Constructor público que FUERZA la integridad al crear la regla
    public TaxRule(int originCountryId, int destCountryId, ProductTaxCategory category, ClientTaxProfile profile, decimal rate, string legalReference)
    {
        // Validaciones de Dominio (Guard Clauses)
        if (rate < 0) throw new ArgumentException("La tasa de impuesto no puede ser negativa.", nameof(rate));
        if (string.IsNullOrWhiteSpace(legalReference)) throw new ArgumentException("Debe existir una referencia legal para auditoría.", nameof(legalReference));

        OriginCountryID = originCountryId;
        DestCountryID = destCountryId;
        ProductTaxCategory = category;
        ClientTaxProfile = profile;
        TaxRate = rate;
        LegalReference = legalReference;
        EffectiveDate = DateTime.UtcNow; // Por defecto, vigente desde hoy
    }

    public int RuleID { get; private set; } // Setter privado, solo la DB genera IDs

    // Nexus: ¿Quién factura?
    public int OriginCountryID { get; private set; }

    // Jurisdicción: ¿Quién compra?
    public int DestCountryID { get; private set; }

    // Vectores de Decisión (Enums para evitar "Magic Strings")
    public ProductTaxCategory ProductTaxCategory { get; private set; }
    public ClientTaxProfile ClientTaxProfile { get; private set; }

    // El Valor Crítico (Decimal para precisión financiera)
    public decimal TaxRate { get; private set; }

    // Auditoría
    public DateTime EffectiveDate { get; private set; }
    public string LegalReference { get; private set; } = string.Empty;

    // Comportamiento: Capacidad de actualizar la tasa (Auditada)
    public void UpdateRate(decimal newRate, string newLegalReference)
    {
        if (newRate < 0) throw new ArgumentException("La tasa no puede ser negativa.");

        TaxRate = newRate;
        LegalReference = newLegalReference;
        EffectiveDate = DateTime.UtcNow; // Reinicia la vigencia con el cambio
    }
}