using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COEM.LicenseIQ.Infrastructure.Persistence.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        // Nombre de la tabla
        builder.ToTable("TaxRulesEngine");

        // Clave Primaria
        builder.HasKey(t => t.RuleID);

        // Configuración de Enums (Conversión a String para legibilidad en DB)
        builder.Property(t => t.ProductTaxCategory)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.ClientTaxProfile)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Precisión Financiera (CRÍTICO: 5 dígitos total, 2 decimales según Spec inicial, 
        // pero mejor usemos decimal(18,4) si queremos consistencia con precios, 
        // aunque para tasas (ej 19.00) (5,2) basta. Usemos (5,2) para tasas según Anexo 2).
        builder.Property(t => t.TaxRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Auditoría
        builder.Property(t => t.LegalReference)
            .HasMaxLength(200)
            .IsRequired();

        // Índices para velocidad (Zenith Performance)
        // Creamos un índice compuesto porque SIEMPRE buscaremos por estos 4 campos juntos.
        builder.HasIndex(t => new { t.OriginCountryID, t.DestCountryID, t.ProductTaxCategory, t.ClientTaxProfile })
            .HasDatabaseName("IX_TaxRules_SearchVector");
    }
}