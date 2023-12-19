using Application.Aggregates;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.PowerPrices;

public record UpdatePowerPricesCommand(PowerPriceAggregate PowerPriceAggregate) : IRequest;

public class UpdatePowerPricesHandler : IRequestHandler<UpdatePowerPricesCommand>
{
    private readonly ILogger<UpdatePowerPricesHandler> _logger;
    private readonly IPowerPriceService _powerPriceService;

    public UpdatePowerPricesHandler(ILogger<UpdatePowerPricesHandler> logger, IPowerPriceService powerPriceService)
    {
        _logger = logger;
        _powerPriceService = powerPriceService;
    }

    public Task Handle(UpdatePowerPricesCommand request, CancellationToken cancellationToken)
    {
        var powerPriceAggregate = request.PowerPriceAggregate;
        _logger.LogInformation("Updating power prices");
        _powerPriceService.UpdatePowerPrices(powerPriceAggregate.PowerPrices);
        return Task.CompletedTask;
    }
}