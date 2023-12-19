using Application.Aggregates;
using Application.ValueObjects;
using Application.Features.HomeAppliances;
using Application.Infrastructure.Factories;
using Microsoft.Extensions.Options;
using Moq;
using Application.Settings;
using Application.Interfaces;

namespace UnitTests.Application.Aggregates;

public class ConsumptionAggregateTests
{
    private static ConsumptionAggregate GetConsumptionAggregate(List<HomeApplianceAggregate> homeAppliances, PowerPriceCategory powerPriceCategory, ActiveEffect activeEffect, AverageConsumption averageConsumption)
    {
        ActiveEffect.SetMaxValue(5);
        AverageConsumption.SetMaxValue(5);

        var homeApplianceAggregateFactoryMock = new Mock<IHomeApplianceAggregateFactory>();
        homeApplianceAggregateFactoryMock.Setup(m => m.HomeAppliances()).Returns(homeAppliances);

        var mockSettings = new Mock<IOptions<ConsumptionSettings>>();
        mockSettings.Setup(m => m.Value).Returns(new ConsumptionSettings
        {
            ActiveEffectMaxValue = 5,
            AverageConsumptionMaxValue = 5,
        });

        var mockPowerPriceService = new Mock<IPowerPriceService>();
        mockPowerPriceService.Setup(m => m.GetCurrentPowerPriceCategory()).Returns(powerPriceCategory);

        var consumptionFactory = new ConsumptionAggregateFactory(homeApplianceAggregateFactoryMock.Object, mockSettings.Object, mockPowerPriceService.Object);

        var consumptionAggregate = consumptionFactory.Create(activeEffect.Value, averageConsumption.Value);

        return consumptionAggregate;
    }

    private static ConsumptionAggregate GetConsumptionAggregateTen(List<HomeApplianceAggregate> homeAppliances, PowerPriceCategory powerPriceCategory, ActiveEffect activeEffect, AverageConsumption averageConsumption)
    {
        ActiveEffect.SetMaxValue(5);
        AverageConsumption.SetMaxValue(5);

        var homeApplianceAggregateFactoryMock = new Mock<IHomeApplianceAggregateFactory>();
        homeApplianceAggregateFactoryMock.Setup(m => m.HomeAppliances()).Returns(homeAppliances);

        var mockSettings = new Mock<IOptions<ConsumptionSettings>>();
        mockSettings.Setup(m => m.Value).Returns(new ConsumptionSettings
        {
            ActiveEffectMaxValue = 10,
            AverageConsumptionMaxValue = 9.9,
        });

        var mockPowerPriceService = new Mock<IPowerPriceService>();
        mockPowerPriceService.Setup(m => m.GetCurrentPowerPriceCategory()).Returns(powerPriceCategory);

        var consumptionFactory = new ConsumptionAggregateFactory(homeApplianceAggregateFactoryMock.Object, mockSettings.Object, mockPowerPriceService.Object);

        var consumptionAggregate = consumptionFactory.Create(activeEffect.Value, averageConsumption.Value);

        return consumptionAggregate;
    }

    [Fact]
    public void Process_LowPrice_TurnOnFirstHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };

        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance1");
    }

    [Fact]
    public void Process_LowPriceWithinActiveEffect_TurnOnFirstHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance1");
    }

    [Fact]
    public void Process_LowPrice_TurnOnSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_TurnOnSecondAfterThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_TurnOnThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_LowPrice_NotTurnOnAnyInactiveHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(3.5);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.5);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_NotTurnOnAnyActiveHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_TurnOffFirstHomeApplianceSinceActiveEffectIsOk()
    {
        // Arrange
        var activeEffect = new ActiveEffect(4.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(5.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_NotTurnOffFirstHomeApplianceSinceEverythingIsOk()
    {
        // Arrange
        var activeEffect = new ActiveEffect(4.9);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(4.8);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_NotTurnOffFirstHomeApplianceSinceEverythingIsOkTen()
    {
        // Arrange
        var activeEffect = new ActiveEffect(9.9);
        ActiveEffect.SetMaxValue(10);
        var averageConsumption = new AverageConsumption(9.8);
        AverageConsumption.SetMaxValue(9.9);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregateTen(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_TurnOffSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(5.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_TurnOffSecondHomeApplianceTen()
    {
        // Arrange
        var activeEffect = new ActiveEffect(11.0);
        ActiveEffect.SetMaxValue(10);
        var averageConsumption = new AverageConsumption(9.9);
        AverageConsumption.SetMaxValue(9.9);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregateTen(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_NoChangeSinceAverageConsumptionIsOk()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(4.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_NoChangeSinceAverageConsumptionIsOkTen()
    {
        // Arrange
        var activeEffect = new ActiveEffect(11.0);
        ActiveEffect.SetMaxValue(10);
        var averageConsumption = new AverageConsumption(9.0);
        AverageConsumption.SetMaxValue(9.9);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregateTen(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_LowPrice_TurnOffSecondHomeApplianceSinceAverageConsumptionIsNotOk()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(6.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_TurnOffSecondHomeApplianceWhenNotAbleToActivateFirst()
    {
        // Arrange
        var activeEffect = new ActiveEffect(4.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(4.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_LowPrice_TurnOffThirdHomeApplianceWhenNotAbleToActivateFirst()
    {
        // Arrange
        var activeEffect = new ActiveEffect(4.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(4.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_LowPrice_TurnOffThirdHomeApplianceWhenNotAbleToActivateFirstAgain()
    {
        // Arrange
        var activeEffect = new ActiveEffect(4.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(4.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_LowPrice_TurnOffThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(5.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_LowPrice_NotTurningOffAnyHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(5.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Low;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_NormalPrice_TurnOnSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_NormalPrice_TurnOnThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_NormalPrice_TurnOnSecondAfterThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_NormalPrice_NotTurningOnAnyHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_NormalPrice_TurnOffFirstHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance1");
    }

    [Fact]
    public void Process_NormalPrice_TurnOffThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(6.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_NormalPrice_TurnOffThirdHomeApplianceAgain()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(6.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_NormalPrice_TurnOffSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(6.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_NormalPrice_NotTurningOffAnyHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(6.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(6.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.Normal;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_HighPrice_TurnOnSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StartHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_HighPrice_TurnOffFirstHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance1");
    }

    [Fact]
    public void Process_HighPrice_TurnOffSecondAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(5.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(5.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance2");
    }

    [Fact]
    public void Process_HighPrice_NotTurnOffSecondHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        ActiveEffect.SetMaxValue(5);
        var averageConsumption = new AverageConsumption(3.0);
        AverageConsumption.SetMaxValue(5);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);
        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }

    [Fact]
    public void Process_HighPrice_TurnOffThirdHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance3", Category.LivingRoom, 4, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Single(consumptionAggregate.DomainCommands);
        Assert.True(consumptionAggregate.DomainCommands.First() is StopHomeApplianceCommand);
        Assert.True(consumptionAggregate.DomainCommands.First().HomeAppliance.Name == "Appliance3");
    }

    [Fact]
    public void Process_HighPrice_NotTurningOffAnyHomeAppliance()
    {
        // Arrange
        var activeEffect = new ActiveEffect(2.0);
        var averageConsumption = new AverageConsumption(3.0);
        var powerPriceCategory = PowerPriceCategory.High;
        var homeAppliances = new List<HomeApplianceAggregate>
        {
            new("Appliance1", Category.LivingRoom, -1, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 2000),
            new("Appliance2", Category.LivingRoom, 0, Status.On, "http://statusUrl", "http://startUrl", "http://stopUrl", 1000),
            new("Appliance3", Category.LivingRoom, 4, Status.Off, "http://statusUrl", "http://startUrl", "http://stopUrl", 1500),
        };
        var consumptionAggregate = GetConsumptionAggregate(homeAppliances, powerPriceCategory, activeEffect, averageConsumption);

        // Act
        consumptionAggregate.Process();

        // Assert
        Assert.Empty(consumptionAggregate.DomainCommands);
    }
}