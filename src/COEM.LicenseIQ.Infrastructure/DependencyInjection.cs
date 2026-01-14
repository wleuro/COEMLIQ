using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Infrastructure.Persistence;
using COEM.LicenseIQ.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace COEM.LicenseIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración de SQL Server
        // La ConnectionString vendrá del appsettings.json o Key Vault
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Registrar Repositorios
        services.AddScoped<ITaxRepository, TaxRepository>();

        return services;
    }
}