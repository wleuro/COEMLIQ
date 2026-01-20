using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using COEM.LicenseIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;

namespace COEM.LicenseIQ.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // --- IMPLEMENTACIÓN BLINDADA (Expression Body) ---
    // Esto conecta la interfaz directamente al motor de EF Core sin ambigüedades.

    public DbSet<COEM.LicenseIQ.Domain.Entities.Country> Countries => Set<COEM.LicenseIQ.Domain.Entities.Country>();

    public DbSet<COEM.LicenseIQ.Domain.Entities.TaxRule> TaxRules => Set<COEM.LicenseIQ.Domain.Entities.TaxRule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);

        // Mapeo a Tablas SQL (Pluralización correcta)
        builder.Entity<COEM.LicenseIQ.Domain.Entities.Country>().ToTable("Countries");
        builder.Entity<COEM.LicenseIQ.Domain.Entities.TaxRule>().ToTable("TaxRules");
    }
}