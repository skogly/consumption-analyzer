using Application.Common;
using Application.Interfaces;
using Application.Strategies;
using Application.ValueObjects;

namespace Application.Aggregates;

public class ConsumptionAggregate
{
    private const int SpecialPriorityLevel = -1;
    private readonly ActiveEffect _activeEffect;
    private readonly AverageConsumption _averageConsumption;
    private readonly List<HomeApplianceAggregate> _homeAppliances;
    private readonly PowerPriceCategory _powerPriceCategory;
    private readonly List<DomainCommand<HomeApplianceAggregate>> _domainCommands = new();
    public ActiveEffect ActiveEffect => _activeEffect;
    public AverageConsumption AverageConsumption => _averageConsumption;
    public PowerPriceCategory PowerPriceCategory => _powerPriceCategory;

    public List<DomainCommand<HomeApplianceAggregate>> DomainCommands => _domainCommands;

    internal ConsumptionAggregate(ActiveEffect activeEffect, AverageConsumption averageConsumption,
                                  PowerPriceCategory powerPriceCategory, List<HomeApplianceAggregate> homeAppliances)
    {
        _activeEffect = activeEffect;
        _averageConsumption = averageConsumption;
        _powerPriceCategory = powerPriceCategory;
        _homeAppliances = homeAppliances;
    }

    public void Process()
    {
        var consumptionStrategy = CreateConsumptionStrategy();
        CheckConsumption(consumptionStrategy);
    }

    private IConsumptionStrategy CreateConsumptionStrategy()
    {
        return _powerPriceCategory switch
        {
            PowerPriceCategory.Low => new LowPowerPriceConsumptionStrategy(this),
            PowerPriceCategory.High => new HighPowerPriceConsumptionStrategy(this),
            _ => new NormalPowerPriceConsumptionStrategy(this),
        };
    }

    private static void CheckConsumption(IConsumptionStrategy consumptionStrategy)
    {
        consumptionStrategy.CheckConsumption();
    }

    public HomeApplianceAggregate? GetSpecialPriorityInactiveHomeAppliance()
    {
        return _homeAppliances.FirstOrDefault(x => x.PriorityLevel == SpecialPriorityLevel && x.Status == Status.Off);
    }

    public HomeApplianceAggregate? GetSpecialPriorityActiveHomeAppliance()
    {
        return _homeAppliances.FirstOrDefault(x => x.PriorityLevel == SpecialPriorityLevel && x.Status == Status.On);
    }

    public HomeApplianceAggregate? GetAnyActiveButPriorityLevelZeroHomeAppliance()
    {
        return _homeAppliances.FirstOrDefault(x => x.Status == Status.On && x.PriorityLevel != 0);
    }

    public HomeApplianceAggregate? GetNormalPriorityInactiveHomeAppliance()
    {
        return GetHomeAppliance(x => x.Status == Status.Off && x.PriorityLevel != SpecialPriorityLevel, false);
    }

    public HomeApplianceAggregate? GetZeroPriorityInactiveHomeAppliance()
    {
        return GetHomeAppliance(x => x.Status == Status.Off && x.PriorityLevel == 0, true);
    }

    public HomeApplianceAggregate? GetNormalPriorityActiveHomeAppliance()
    {
        return GetHomeAppliance(x => x.Status == Status.On && x.PriorityLevel != SpecialPriorityLevel, true);
    }

    private HomeApplianceAggregate? GetHomeAppliance(Func<HomeApplianceAggregate, bool> predicate, bool orderByDescending)
    {
        var appliances = _homeAppliances.Where(predicate);
        appliances = orderByDescending ? appliances.OrderByDescending(x => x.PriorityLevel) : appliances.OrderBy(x => x.PriorityLevel);
        return appliances.FirstOrDefault();
    }
}
