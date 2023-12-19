using Application.Aggregates;
using Application.Infrastructure.Http;
using Application.Interfaces;
using Application.Settings;
using Microsoft.Extensions.Options;

namespace Application.Infrastructure.Factories;

public class HomeApplianceAggregateFactory : IHomeApplianceAggregateFactory
{
    private readonly HomeApplianceSettings _homeApplianceSettings;
    private readonly IHttpHandler _httpHandler;
    private List<HomeApplianceAggregate> _homeAppliances;
    public List<HomeApplianceAggregate> HomeAppliances() => _homeAppliances;

    public HomeApplianceAggregateFactory(IOptions<HomeApplianceSettings> homeApplianceSettings, IHttpHandler httpHandler)
    {
        _homeApplianceSettings = homeApplianceSettings.Value;
        _httpHandler = httpHandler;
        _homeAppliances = InitializeHomeAppliances();
    }

    public List<HomeApplianceAggregate> InitializeHomeAppliances()
    {
        if (_homeApplianceSettings.HomeAppliances == null)
        {
            return new List<HomeApplianceAggregate>();
        }

        var homeAppliances = _homeApplianceSettings.HomeAppliances.Where(x => x.Status != "Disabled").Select(homeAppliance =>
            {
                var statusState = new Status();
                if (homeAppliance.Status != "Disabled")
                {
                    var uri = new Uri(homeAppliance.StatusUrl);
                    statusState = _httpHandler.GetAsync<HomeApplianceDto>(uri).Result.OnState switch
                    {
                        "true" => Status.On,
                        "false" => Status.Off,
                        _ => Status.Disabled
                    };
                }
                else
                {
                    Enum.TryParse<Status>(homeAppliance.Status, true, out var newStatusState);
                    statusState = newStatusState;
                }
                Enum.TryParse<Category>(homeAppliance.Category, true, out var category);
                return new HomeApplianceAggregate(homeAppliance.Name, category, homeAppliance.PriorityLevel, statusState, homeAppliance.StatusUrl, homeAppliance.StartUrl, homeAppliance.StopUrl, homeAppliance.Wattage);
            });

        return homeAppliances.Where(x => x.Status != Status.Disabled).ToList();
    }
}

public class HomeApplianceDto
{
    public string? Name { get; init; }
    public int Key { get; init; }
    public string? OnState { get; init; }
    public string? Hue { get; init; }
}