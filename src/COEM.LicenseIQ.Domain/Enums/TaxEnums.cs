using System;
using System.Collections.Generic;
using System.Text;

namespace COEM.LicenseIQ.Domain.Enums;

//public enum ProductTaxCategory
//{
//    Cloud = 1,          // 0% en muchos casos
//    OnPremise = 2,      // 19% estándar
//    Hardware = 3,       // Físico
//    ProfessionalServices = 4 // Consultoría
//}

//public enum ClientTaxProfile
//{
//    Standard = 1,           // Régimen Común
//    GranContribuyente = 2,  // Auto-retenedor
//    ZonaFranca = 3,         // Exento
//    GovernmentExempt = 4    // Entidades públicas especiales
//}

public enum ProductTaxCategory
{
    // Categoría 1: Intangibles Nube (Office 365, Azure, Dynamics)
    // Comportamiento: 0% IVA en Colombia (bajo condiciones de ley), Gravado en otros países.
    Cloud = 1,

    // Categoría 2: Software Instalable/Perpetuo (Windows Server, SQL Server)
    // Comportamiento: Siempre Gravado (19% CO, 18% PE, 15% EC).
    Software_Local = 2
}