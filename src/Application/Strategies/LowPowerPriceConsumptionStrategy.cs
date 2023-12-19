using Application.Aggregates;
using Application.Interfaces;

namespace Application.Strategies;

public class LowPowerPriceConsumptionStrategy : IConsumptionStrategy
{
    private readonly ConsumptionAggregate _consumptionAggregate;

    public LowPowerPriceConsumptionStrategy(ConsumptionAggregate consumptionAggregate)
    {
        _consumptionAggregate = consumptionAggregate;
    }

    public void CheckConsumption()
    {
        if (TryStartSpecialPriorityAppliance() || TryStopAppliance() || TryStartNormalAppliance())
        {
            return;
        }
    }

    private bool TryStartSpecialPriorityAppliance()
    {
        var homeAppliance = _consumptionAggregate.GetSpecialPriorityInactiveHomeAppliance();

        if (homeAppliance != null && !_consumptionAggregate.ActiveEffect.IsOver(homeAppliance.KiloWatts))
        {
            _consumptionAggregate.DomainCommands.Add(homeAppliance.CreateStartHomeApplianceCommand());
            System.Console.WriteLine($"Starting {homeAppliance.Name} with special priority");
            return true;
        }

        return false;
    }

    private bool TryStopAppliance()
    {
        if (!_consumptionAggregate.AverageConsumption.IsOver() && _consumptionAggregate.GetSpecialPriorityActiveHomeAppliance()?.Status == Status.On)
        {
            return false;
        }

        var activeHomeAppliance = _consumptionAggregate.GetNormalPriorityActiveHomeAppliance();

        if (activeHomeAppliance != null)
        {
            _consumptionAggregate.DomainCommands.Add(activeHomeAppliance.CreateStopHomeApplianceCommand());
            System.Console.WriteLine($"Stopping {activeHomeAppliance.Name}");
            return true;
        }

        if (!_consumptionAggregate.ActiveEffect.IsOver())
        {
            return false;
        }

        activeHomeAppliance = _consumptionAggregate.GetSpecialPriorityActiveHomeAppliance();

        if (activeHomeAppliance != null)
        {
            _consumptionAggregate.DomainCommands.Add(activeHomeAppliance.CreateStopHomeApplianceCommand());
            return true;
        }

        return false;
    }

    private bool TryStartNormalAppliance()
    {
        var inactiveHomeAppliance = _consumptionAggregate.GetNormalPriorityInactiveHomeAppliance();

        if (inactiveHomeAppliance != null && !_consumptionAggregate.ActiveEffect.IsOver(inactiveHomeAppliance.KiloWatts) && _consumptionAggregate.GetSpecialPriorityActiveHomeAppliance()?.Status == Status.On)
        {
            _consumptionAggregate.DomainCommands.Add(inactiveHomeAppliance.CreateStartHomeApplianceCommand());
            System.Console.WriteLine($"Starting {inactiveHomeAppliance.Name}");
            return true;
        }

        return false;
    }
}
