using MediatR;

namespace Application.Common;

public record DomainCommand<TAggregate> : IRequest
{
    public TAggregate HomeAppliance { get; init; }

    public DomainCommand(TAggregate homeAppliance)
    {
        HomeAppliance = homeAppliance;
    }
}