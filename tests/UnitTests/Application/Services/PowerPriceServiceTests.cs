using Xunit;
using Application.Infrastructure.Services;
using Application.ValueObjects;

namespace UnitTests.Application.Services;

public class PowerPriceServiceTests
{
    [Fact]
    public void UpdatePowerPrices_WhenNoPowerPrices_ReturnEmptyList()
    {
        var powerPrices = new List<PowerPrice>();

        var powerPriceService = new PowerPriceService();

        powerPriceService.UpdatePowerPrices(powerPrices);
    }
}