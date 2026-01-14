using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using COEM.LicenseIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace COEM.LicenseIQ.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TaxRule> TaxRules => Set<TaxRule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Aplica automáticamente todas las configuraciones (como TaxRuleConfiguration)
        // que encuentre en este ensamblado.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}