using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Infrastructure.Persistence;
using COEM.LicenseIQ.Infrastructure.Persistence.Repositories;
using COEM.LicenseIQ.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        // Registrar Repositorios
        services.AddScoped<ITaxRepository, TaxRepository>();
        services.AddScoped<ICommonRepository, CommonRepository>();
        services.AddScoped<IPriceIngestionService, PriceIngestionService>();
        services.AddScoped<QuoteExportService>();
        return services;
    }
}