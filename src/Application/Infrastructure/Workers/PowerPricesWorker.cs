using System.Globalization;
using Application.Features.PowerPrices;
using Application.Infrastructure.Dtos;
using Application.Infrastructure.Factories;
using Application.Infrastructure.Http;
using Application.Interfaces;
using Application.Settings;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Infrastructure.Workers;

public class PowerPricesWorker : BackgroundService
{
    private DateTime _dateTimeUpdated = DateTime.Now.AddDays(-1);
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<PowerPricesWorker> _logger;
    private readonly PowerPriceAggregateFactory _powerPriceAggregateFactory;
    private readonly IMediator _mediator;
    private readonly HttpSettings _httpSettings;

    public PowerPricesWorker(IHttpHandler httpHandler, IMediator mediator, IOptions<HttpSettings> httpSettings,
                             PowerPriceAggregateFactory powerPriceAggregateFactory, ILogger<PowerPricesWorker> logger)
    {
        _httpHandler = httpHandler;
        _powerPriceAggregateFactory = powerPriceAggregateFactory;
        _logger = logger;
        _mediator = mediator;
        _httpSettings = httpSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsNewDay())
            {
                _logger.LogInformation("New day!");
                await UpdatePrices().ConfigureAwait(false);
            }
            await Task.Delay(60000, stoppingToken).ConfigureAwait(false);
        }
    }

    private bool IsNewDay()
    {
        return DateTime.Now.ToLocalTime().Day != _dateTimeUpdated.Day;
    }

    private async Task UpdatePrices()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ", CultureInfo.InvariantCulture);
        var uri = new Uri($"{_httpSettings.PowerPriceApiUrl}/pricesDay?date={today}");
        var prices = await _httpHandler.GetAsync<List<PowerPriceDto>>(uri).ConfigureAwait(false);
        var powerPriceAggregate = _powerPriceAggregateFactory.Create(prices);
        var command = new UpdatePowerPricesCommand(powerPriceAggregate);
        await _mediator.Send(command).ConfigureAwait(false);
        _dateTimeUpdated = DateTime.Now.ToLocalTime();
    }
}