using Application.Aggregates;
using Application.Infrastructure.Dtos;
using Application.Settings;
using Application.ValueObjects;
using Microsoft.Extensions.Options;

namespace Application.Infrastructure.Factories;

public class PowerPriceAggregateFactory
{
    private readonly PowerPriceSettings _powerPriceSettings;

    public PowerPriceAggregateFactory(IOptions<PowerPriceSettings> ioptionsPowerPriceSettings)
    {
        _powerPriceSettings = ioptionsPowerPriceSettings.Value;
    }

    public PowerPriceAggregate Create(List<PowerPriceDto> powerPriceDtos)
    {
        var powerPrices = powerPriceDtos.Select(dto => new PowerPrice(dto.TimeStamp.ToLocalTime().Hour, dto.Price)).ToList();
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, _powerPriceSettings);
        powerPriceAggregate.FindLowestHours();
        powerPriceAggregate.FindHighestHours();
        return powerPriceAggregate;
    }
}