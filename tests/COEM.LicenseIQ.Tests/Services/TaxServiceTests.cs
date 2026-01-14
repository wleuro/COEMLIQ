using System;
using System.Collections.Generic;
using System.Text;
using COEM.LicenseIQ.Application.Common.Interfaces.Persistence;
using COEM.LicenseIQ.Application.Services;
using COEM.LicenseIQ.Domain.Entities;
using COEM.LicenseIQ.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace COEM.LicenseIQ.Tests.Services;

public class TaxServiceTests
{
    private readonly Mock<ITaxRepository> _mockRepo;
    private readonly Mock<ILogger<TaxService>> _mockLogger;
    private readonly TaxService _taxService;

    public TaxServiceTests()
    {
        // 1. Arrange: Preparamos los dobles de riesgo (Mocks)
        _mockRepo = new Mock<ITaxRepository>();
        _mockLogger = new Mock<ILogger<TaxService>>();

        // Inyectamos los mocks en el servicio real
        _taxService = new TaxService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateTax_ShouldReturnZeroTax_WhenRuleSaysCloudIsExempt()
    {
        // ARRANGE (Preparación)
        var originId = 1; // Colombia
        var destId = 1;   // Colombia
        var category = ProductTaxCategory.Cloud;
        var profile = ClientTaxProfile.Standard;
        var baseAmount = 1000m; // $1,000 USD

        // Simulamos que la DB encuentra una regla del 0%
        var expectedRule = new TaxRule(originId, destId, category, profile, 0.00m, "Estatuto Tributario Art 476");

        _mockRepo.Setup(r => r.GetMatchAsync(originId, destId, category, profile, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(expectedRule);

        // ACT (Ejecución)
        var result = await _taxService.CalculateTaxAsync(originId, destId, category, profile, baseAmount, CancellationToken.None);

        // ASSERT (Verificación)
        Assert.True(result.IsExactMatch);          // Encontró regla
        Assert.Equal(0.00m, result.Rate);          // Tasa 0%
        Assert.Equal(0.00m, result.TaxAmount);     // Monto 0
        Assert.Equal(expectedRule.RuleID, result.RuleId); // Trazabilidad correcta
    }

    [Fact]
    public async Task CalculateTax_ShouldApplyFallback_WhenNoRuleExists()
    {
        // ARRANGE
        // Simulamos un escenario raro: Hardware vendido desde Colombia a un país desconocido
        var originId = 1;
        var destId = 99;
        var category = ProductTaxCategory.Hardware;
        var profile = ClientTaxProfile.Standard;
        var baseAmount = 1000m;

        // Simulamos que la DB NO encuentra nada (devuelve null)
        _mockRepo.Setup(r => r.GetMatchAsync(originId, destId, category, profile, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TaxRule?)null);

        // ACT
        var result = await _taxService.CalculateTaxAsync(originId, destId, category, profile, baseAmount, CancellationToken.None);

        // ASSERT
        Assert.False(result.IsExactMatch); // Bandera roja levantada
        Assert.Contains("FALLBACK", result.AppliedRuleReference); // Referencia indica fallback

        // Según tu lógica actual en TaxService: si origin != dest, fallback es 0%
        Assert.Equal(0.00m, result.Rate);
    }

    [Fact]
    public async Task CalculateTax_ShouldCalculateVAT_WhenRuleSays19Percent()
    {
        // ARRANGE
        var originId = 1;
        var destId = 1;
        var category = ProductTaxCategory.OnPremise;
        var profile = ClientTaxProfile.Standard;
        var baseAmount = 100.00m;

        // Regla del 19%
        var rule = new TaxRule(originId, destId, category, profile, 19.00m, "Ley General IVA");

        _mockRepo.Setup(r => r.GetMatchAsync(originId, destId, category, profile, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(rule);

        // ACT
        var result = await _taxService.CalculateTaxAsync(originId, destId, category, profile, baseAmount, CancellationToken.None);

        // ASSERT
        Assert.Equal(19.00m, result.TaxAmount); // 19% de 100 es 19
    }
}