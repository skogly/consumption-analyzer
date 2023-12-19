using Application.Aggregates;
using Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Consumption;

public record CheckConsumptionCommand(ConsumptionAggregate ConsumptionAggregate) : IRequest<ConsumptionAggregate>;

public class CheckConsumptionAggregateHandler : IRequestHandler<CheckConsumptionCommand, ConsumptionAggregate>
{
    private const int MinutesToWaitAfterUpdate = 5;
    private readonly ILogger<CheckConsumptionAggregateHandler> _logger;
    private readonly IMediator _mediatr;
    private static DateTime _lastCheckedDateTime = DateTime.MinValue;

    public CheckConsumptionAggregateHandler(ILogger<CheckConsumptionAggregateHandler> logger, IMediator mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    public async Task<ConsumptionAggregate> Handle(CheckConsumptionCommand request, CancellationToken cancellationToken)
    {
        var consumptionAggregate = request.ConsumptionAggregate;

        if (IsTimeToUpdate())
        {
            consumptionAggregate.Process();
            await SendDomainCommands(consumptionAggregate, cancellationToken).ConfigureAwait(false);
        }

        return consumptionAggregate;
    }

    private static bool IsTimeToUpdate()
    {
        return _lastCheckedDateTime.AddMinutes(MinutesToWaitAfterUpdate) < DateTime.Now;
    }

    private async Task SendDomainCommands(ConsumptionAggregate consumptionAggregate, CancellationToken cancellationToken)
    {
        foreach (var domainCommand in consumptionAggregate.DomainCommands)
        {
            await _mediatr.Send(domainCommand, cancellationToken).ConfigureAwait(false);
            LogCommand(domainCommand, consumptionAggregate);
            _lastCheckedDateTime = DateTime.Now;
        }
    }

    private void LogCommand(DomainCommand<HomeApplianceAggregate> domainCommand, ConsumptionAggregate consumptionAggregate)
    {
        _logger.LogInformation("Command: {HomeApplianceName} - {CommandType} - {PowerPriceCategory} price - {ActiveEffectValue} kW - {AverageConsumptionValue} kWh",
            domainCommand.HomeAppliance.Name,
            domainCommand.GetType().Name,
            consumptionAggregate.PowerPriceCategory,
            consumptionAggregate.ActiveEffect.Value,
            consumptionAggregate.AverageConsumption.Value);
    }
}