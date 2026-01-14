using System;
using System.Collections.Generic;
using System.Text;

namespace COEM.LicenseIQ.Domain.Enums;

public enum ProductTaxCategory
{
    Cloud = 1,          // 0% en muchos casos
    OnPremise = 2,      // 19% estándar
    Hardware = 3,       // Físico
    ProfessionalServices = 4 // Consultoría
}

public enum ClientTaxProfile
{
    Standard = 1,           // Régimen Común
    GranContribuyente = 2,  // Auto-retenedor
    ZonaFranca = 3,         // Exento
    GovernmentExempt = 4    // Entidades públicas especiales
}