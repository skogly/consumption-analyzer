using Application.Aggregates;
using Application.Interfaces;

namespace Application.Strategies;

public class HighPowerPriceConsumptionStrategy : IConsumptionStrategy
{
    private readonly ConsumptionAggregate _consumptionAggregate;

    public HighPowerPriceConsumptionStrategy(ConsumptionAggregate consumptionAggregate)
    {
        _consumptionAggregate = consumptionAggregate;
    }

    public void CheckConsumption()
    {
        if (TryStopAnyRunningAppliance() || TryStopApplianceExceedingAverageConsumption() || TryStartNormalInactiveAppliance())
        {
            return;
        }
    }

    private bool TryStopAnyRunningAppliance()
    {
        var runningHomeAppliance = _consumptionAggregate.GetAnyActiveButPriorityLevelZeroHomeAppliance();

        if (runningHomeAppliance != null)
        {
            _consumptionAggregate.DomainCommands.Add(runningHomeAppliance.CreateStopHomeApplianceCommand());
            return true;
        }

        return false;
    }

    private bool TryStopApplianceExceedingAverageConsumption()
    {
        if (!_consumptionAggregate.ActiveEffect.IsOver() || !_consumptionAggregate.AverageConsumption.IsOver())
        {
            return false;
        }

        var activeHomeAppliance = _consumptionAggregate.GetNormalPriorityActiveHomeAppliance();

        if (activeHomeAppliance != null)
        {
            _consumptionAggregate.DomainCommands.Add(activeHomeAppliance.CreateStopHomeApplianceCommand());
            return true;
        }

        return false;
    }

    private bool TryStartNormalInactiveAppliance()
    {
        var inactiveHomeAppliance = _consumptionAggregate.GetZeroPriorityInactiveHomeAppliance();

        if (inactiveHomeAppliance != null && !_consumptionAggregate.ActiveEffect.IsOver(inactiveHomeAppliance.KiloWatts))
        {
            _consumptionAggregate.DomainCommands.Add(inactiveHomeAppliance.CreateStartHomeApplianceCommand());
            return true;
        }

        return false;
    }
}