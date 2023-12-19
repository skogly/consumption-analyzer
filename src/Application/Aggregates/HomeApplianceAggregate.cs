using Application.Features.HomeAppliances;
using Application.ValueObjects;

namespace Application.Aggregates;

public class HomeApplianceAggregate
{
    public string Name { get; init; }
    public Category Category { get; init; }
    public int PriorityLevel { get; private set; }
    private Status _status { get; set; }
    public Url StatusUrl { get; init; }
    public Url StartUrl { get; init; }
    public Url StopUrl { get; init; }
    public double KiloWatts { get; init; }

    public HomeApplianceAggregate(string name, Category category, int priorityLevel, Status status,
                                  string statusUrl, string startUrl, string stopUrl, int wattage)
    {
        Name = name;
        Category = category;
        PriorityLevel = priorityLevel;
        _status = status;
        StatusUrl = new Url(statusUrl);
        StartUrl = new Url(startUrl);
        StopUrl = new Url(stopUrl);
        KiloWatts = (double)wattage / 1000;
        System.Console.WriteLine($"Created {name} with status {Status}");
    }

    public void TurnOn()
    {
        Status = Status.On;
    }

    public void TurnOff()
    {
        Status = Status.Off;
    }

    public Status Status 
    { 
        get => _status; 
        private set 
        {
            _status = value;
            System.Diagnostics.Debug.WriteLine($"Status of {Name} changed to {_status}");
        }
    }

    public StartHomeApplianceCommand CreateStartHomeApplianceCommand()
    {
        return new StartHomeApplianceCommand(this);
    }

    public StopHomeApplianceCommand CreateStopHomeApplianceCommand()
    {
        return new StopHomeApplianceCommand(this);
    }
}

public enum Category
{
    Entrance,
    BathroomDownstairs,
    LivingRoom,
    Kitchen,
    StorageRoomDownstairs,
    Stairs,
    HallwayUpstairs,
    StorageRoomUpstairs,
    Purple,
    Gym,
    BathroomUpStairs,
    Bedroom
}

public enum Status
{
    On,
    Off,
    Disabled
}