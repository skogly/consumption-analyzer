using Application.Features.Consumption;
using Application.Interfaces;
using Application.Settings;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Application.Infrastructure.Workers;

public class ConsumptionWorker : BackgroundService
{
    private readonly IManagedMqttClient _mqttClient;
    private readonly ManagedMqttClientOptions _options;
    private readonly ILogger<ConsumptionWorker> _logger;
    private readonly IMediator _mediatr;
    private readonly IConsumptionAggregateFactory _consumptionFactory;
    private double _activePower = -1;
    private double _estimatedConsumption = -1;

    public ConsumptionWorker(ILogger<ConsumptionWorker> logger, IMediator mediatr,
                            IOptions<HttpSettings> httpSettings, IConsumptionAggregateFactory consumptionFactory)
    {
        _logger = logger;
        if (String.IsNullOrEmpty(httpSettings.Value.MqttBroker))
        {
            _logger.LogError("MQTT Broker is not configured");
        }
        _mediatr = mediatr;
        _consumptionFactory = consumptionFactory;
        _options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId("cost-analyzer-test2")
                .WithTcpServer(httpSettings.Value.MqttBroker)
                .Build())
            .Build();
        _logger.LogInformation("Consumption worker is initializing");
        _mqttClient = new MqttFactory().CreateManagedMqttClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _mqttClient.StartAsync(_options).ConfigureAwait(false);

        await _mqttClient.SubscribeAsync(new List<MQTTnet.Packets.MqttTopicFilter>() {
            new MqttTopicFilterBuilder().WithTopic("active_power").Build(),
            new MqttTopicFilterBuilder().WithTopic("estimated_consumption").Build()
        }).ConfigureAwait(false);

        _mqttClient.ConnectedAsync += delegate (MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("Connected to MQTT Broker!");
            return Task.CompletedTask;
        };

        _mqttClient.ConnectingFailedAsync += delegate (ConnectingFailedEventArgs args)
        {
            _logger.LogError("Connection to MQTT broker failed");
            return Task.CompletedTask;
        };

        _mqttClient.ApplicationMessageReceivedAsync += async delegate (MqttApplicationMessageReceivedEventArgs args)
        {
            if (args.ApplicationMessage.Topic == "active_power")
            {
                _activePower = ReadValue(args);
            }
            else if (args.ApplicationMessage.Topic == "estimated_consumption")
            {
                _estimatedConsumption = ReadValue(args);
                var consumptionAggregate = _consumptionFactory.Create(_activePower, _estimatedConsumption);
                var command = new CheckConsumptionCommand(consumptionAggregate);
                await _mediatr.Send(command).ConfigureAwait(false);
            }
        };

        _mqttClient.DisconnectedAsync += delegate (MqttClientDisconnectedEventArgs args)
        {
            _logger.LogWarning("Disconnected from MQTT broker");
            return Task.CompletedTask;
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Disconnecting from MQTT Broker");
        await _mqttClient.StopAsync().ConfigureAwait(false);
    }

    private static double ReadValue(MqttApplicationMessageReceivedEventArgs args)
    {
        return Convert.ToDouble(args.ApplicationMessage.ConvertPayloadToString());
    }
}