using Application.Aggregates;
using Application.Common;
using Application.Infrastructure.Http;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.HomeAppliances;

public record StopHomeApplianceCommand(HomeApplianceAggregate HomeApplianceAggregate) : DomainCommand<HomeApplianceAggregate>(HomeApplianceAggregate);


public class StopHomeApplianceCommandHandler : IRequestHandler<StopHomeApplianceCommand>
{
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<StopHomeApplianceCommandHandler> _logger;

    public StopHomeApplianceCommandHandler(IHttpHandler httpHandler, ILogger<StopHomeApplianceCommandHandler> logger)
    {
        _httpHandler = httpHandler;
        _logger = logger;
    }

    public async Task Handle(StopHomeApplianceCommand request, CancellationToken cancellationToken)
    {
        var result = await _httpHandler.GetAsync<string>(request.HomeAppliance.StopUrl).ConfigureAwait(false);
        if (result.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            request.HomeAppliance.TurnOff();
        }
        else
        {
            _logger.LogError("Could not stop appliance!");
        }
    }
}