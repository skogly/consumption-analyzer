using Application.Settings;
using Microsoft.Extensions.Options;
using Application.Infrastructure.Factories;
using Application.Infrastructure.Dtos;

namespace UnitTests.Application.Factories;

public class PowerPriceFactoryTests
{
    [Fact]
    public void Constructor_WhenNoPowerPrices_ReturnEmptyList()
    {
        var powerPriceSettings = new PowerPriceSettings
        {
            NumberOfLowPriceHours = 2,
            NumberOfHighPriceHours = 2
        };

        var powerPriceAggregateFactory = new PowerPriceAggregateFactory(Options.Create(powerPriceSettings));

        var powerPriceAggregate = powerPriceAggregateFactory.Create(new List<PowerPriceDto>());
    }
}