using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COEM.LicenseIQ.Infrastructure.Persistence.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRulesEngine");
        builder.HasKey(t => t.RuleID);

        builder.Property(t => t.ProductTaxCategory)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.TaxRate).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(t => t.LegalReference).HasMaxLength(200).IsRequired();

        // ÍNDICE FINAL: País + Producto. Búsqueda instantánea.
        builder.HasIndex(t => new { t.CountryID, t.ProductTaxCategory })
            .HasDatabaseName("IX_TaxRules_JurisdictionVector")
            .IsUnique(true);
    }
}