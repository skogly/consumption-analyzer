using Application.ValueObjects;
using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Consumption;
using Application.Aggregates;
using Application.Settings;
using Microsoft.Extensions.Options;
using Application.Interfaces;
using Application.Infrastructure.Factories;

namespace UnitTests.Application.Handlers;

public class CheckConsumptionTests
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

    [Fact]
    public async Task Handle_ShouldSendDomainCommands_WhenTimeToUpdate()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockLogger = new Mock<ILogger<CheckConsumptionAggregateHandler>>();
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

        var request = new CheckConsumptionCommand(consumptionAggregate);
        var handler = new CheckConsumptionAggregateHandler(mockLogger.Object, mockMediator.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMediator.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(consumptionAggregate.DomainCommands.Count));
    }

    [Fact]
    public async Task Handle_ShouldSendDomainCommands_OnlyOnceInTimeFrame()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockLogger = new Mock<ILogger<CheckConsumptionAggregateHandler>>();
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

        var request = new CheckConsumptionCommand(consumptionAggregate);
        var handler = new CheckConsumptionAggregateHandler(mockLogger.Object, mockMediator.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMediator.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(consumptionAggregate.DomainCommands.Count));

        mockMediator = new Mock<IMediator>();


        // Act
        result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockMediator.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
    }
}