namespace Application.Settings;

public class HomeApplianceSettings
{
    public HomeApplianceDto[]? HomeAppliances { get; set; }
}

public class HomeApplianceDto
{
    public string Name { get; init; } = "no_name";
    public string Status { get; set; } = "no_status";
    public string Category { get; init; } = "no_category";
    public int PriorityLevel { get; init; }
    public string StatusUrl { get; init; } = "no_status_url";
    public string StartUrl { get; init; } = "no_start_url";
    public string StopUrl { get; init; } = "no_stop_url";
    public int Wattage { get; init; }
}
