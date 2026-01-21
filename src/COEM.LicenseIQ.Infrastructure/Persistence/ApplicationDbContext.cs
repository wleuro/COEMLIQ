using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Entities.CSP;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace COEM.LicenseIQ.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CSP_Products> CSP_Products { get; set; }
    public DbSet<CSP_PriceList> CSP_PriceList { get; set; }
    public DbSet<PriceListImports> PriceListImports { get; set; }
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