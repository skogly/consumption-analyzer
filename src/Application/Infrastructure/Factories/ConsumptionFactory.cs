using Application.Aggregates;
using Application.Interfaces;
using Application.Settings;
using Application.ValueObjects;
using Microsoft.Extensions.Options;

namespace Application.Infrastructure.Factories;

public class ConsumptionAggregateFactory : IConsumptionAggregateFactory
{
    private readonly ConsumptionSettings _consumptionSettings;
    private readonly IPowerPriceService _powerPriceService;
    private readonly List<HomeApplianceAggregate> _homeAppliances;

    public ConsumptionAggregateFactory(IHomeApplianceAggregateFactory homeApplianceAggregateFactory, IOptions<ConsumptionSettings> ioptionsConsumptionSettings,
                                    IPowerPriceService powerPriceService)
    {
        _consumptionSettings = ioptionsConsumptionSettings.Value;
        _homeAppliances = homeApplianceAggregateFactory.HomeAppliances();
        ActiveEffect.SetMaxValue(_consumptionSettings.ActiveEffectMaxValue);
        AverageConsumption.SetMaxValue(_consumptionSettings.AverageConsumptionMaxValue);
        _powerPriceService = powerPriceService;
    }

    public ConsumptionAggregate Create(double activeEffect, double averageConsumption)
    {
        var powerPriceCategory = _powerPriceService.GetCurrentPowerPriceCategory();
        return new ConsumptionAggregate(new ActiveEffect(activeEffect), new AverageConsumption(averageConsumption), powerPriceCategory, _homeAppliances);
    }
}
