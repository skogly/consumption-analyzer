using Application.Aggregates;
using Application.Interfaces;

namespace Application.Strategies;

public class NormalPowerPriceConsumptionStrategy : IConsumptionStrategy
{
    private readonly ConsumptionAggregate _consumptionAggregate;

    public NormalPowerPriceConsumptionStrategy(ConsumptionAggregate consumptionAggregate)
    {
        _consumptionAggregate = consumptionAggregate;
    }

    public void CheckConsumption()
    {
        if (TryStopSpecialPriorityApplianceRunningOnNormalPrice() || TryStopApplianceExceedingAverageConsumption() || TryStartNormalInactiveAppliance())
        {
            return;
        }
    }

    private bool TryStopSpecialPriorityApplianceRunningOnNormalPrice()
    {
        var specialRunningHomeAppliance = _consumptionAggregate.GetSpecialPriorityActiveHomeAppliance();

        if (specialRunningHomeAppliance != null)
        {
            _consumptionAggregate.DomainCommands.Add(specialRunningHomeAppliance.CreateStopHomeApplianceCommand());
            return true;
        }

        return false;
    }

    private bool TryStartNormalInactiveAppliance()
    {
        var inactiveHomeAppliance = _consumptionAggregate.GetNormalPriorityInactiveHomeAppliance();

        if (inactiveHomeAppliance != null && !_consumptionAggregate.ActiveEffect.IsOver(inactiveHomeAppliance.KiloWatts))
        {
            _consumptionAggregate.DomainCommands.Add(inactiveHomeAppliance.CreateStartHomeApplianceCommand());
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
}