using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Services;
using COEM.LicenseIQ.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace COEM.LicenseIQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Aquí registramos todos los servicios de la capa de Aplicación

        // Registramos el TaxService
        // Scoped = Se crea una instancia nueva por cada solicitud HTTP (Ideal para servicios de negocio)
        services.AddScoped<ITaxService, TaxService>();

        // Aquí agregarás IncentiveService, ClientService, etc. en el futuro.

        return services;
    }
}