using Application.Aggregates;
using Application.Infrastructure.Dtos;
using Application.Infrastructure.Http;
using Application.Interfaces;
using Application.ValueObjects;
using MediatR;

namespace Application.Features.HomeAppliances;

public sealed record GetHomeApplianceStatusCommand(Url Url) : IRequest<Status>;

public class GetHomeApplianceStatusHandler : IRequestHandler<GetHomeApplianceStatusCommand, Status>
{
    private readonly IHttpHandler _httpHandler;

    public GetHomeApplianceStatusHandler(IHttpHandler httpHandler)
    {
        _httpHandler = httpHandler;
    }

    public async Task<Status> Handle(GetHomeApplianceStatusCommand request, CancellationToken cancellationToken)
    {
        var response = await _httpHandler.GetAsync<HomeApplianceStatus>(request.Url).ConfigureAwait(false);

        var status = response.OnState ? Status.On : Status.Off;

        return status;
    }
}
