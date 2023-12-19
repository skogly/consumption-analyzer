using Application.Aggregates;
using Application.Common;
using Application.Infrastructure.Http;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.HomeAppliances;

public record StartHomeApplianceCommand(HomeApplianceAggregate HomeApplianceAggregate) : DomainCommand<HomeApplianceAggregate>(HomeApplianceAggregate);

public class StartHomeApplianceCommandHandler : IRequestHandler<StartHomeApplianceCommand>
{
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<StartHomeApplianceCommandHandler> _logger;

    public StartHomeApplianceCommandHandler(IHttpHandler httpHandler, ILogger<StartHomeApplianceCommandHandler> logger)
    {
        _httpHandler = httpHandler;
        _logger = logger;
    }

    public async Task Handle(StartHomeApplianceCommand request, CancellationToken cancellationToken)
    {
        var result = await _httpHandler.GetAsync<string>(request.HomeAppliance.StartUrl).ConfigureAwait(false);
        if (result.Contains("started", StringComparison.OrdinalIgnoreCase))
        {
            request.HomeAppliance.TurnOn();
        }
        else
        {
            _logger.LogError("Could not start appliance!");
        }
    }
}