using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace COEM.LicenseIQ.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    // Usamos el nombre completo para evitar CUALQUIER duda del compilador
    DbSet<COEM.LicenseIQ.Domain.Entities.Country> Countries { get; }

    // AQUÍ ESTÁ EL CAMPO QUE FALTABA
    DbSet<COEM.LicenseIQ.Domain.Entities.TaxRule> TaxRules { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}