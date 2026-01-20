using System;
using System.Collections.Generic;
using System.Text;
namespace COEM.LicenseIQ.Domain.Entities;

public class Country
{
    // Constructor vacío para EF Core
    private Country() { }

    public Country(string name, string isoCode, string currencyCode)
    {
        Name = name;
        IsoCode = isoCode; // "CO", "PE"
        CurrencyCode = currencyCode; // "COP", "USD"
        IsActive = true;
    }

    public int CountryID { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string IsoCode { get; private set; } = string.Empty;
    public string CurrencyCode { get; private set; } = string.Empty;
    public bool IsActive { get; set; }
}