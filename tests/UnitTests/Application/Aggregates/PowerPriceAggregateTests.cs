using Application.Aggregates;
using Application.Settings;
using Application.ValueObjects;

namespace UnitTests.Application.Aggregates;

public class PowerPriceAggregateTests
{
    [Fact]
    public void FindLowestHours_WhenCalled_FindsTwoLowestHours()
    {
        // Arrange
        var powerPrices = new List<PowerPrice>
        {
            new(0, 10),
            new(1, 20),
            new(2, 30),
            new(3, 40),
            new(4, 50)
        };
        var powerPriceSettings = new PowerPriceSettings { NumberOfLowPriceHours = 2 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        powerPriceAggregate.FindLowestHours();

        // Assert
        Assert.Equal(new List<int> { 0, 1 }, powerPriceAggregate.PowerPrices.Where(x => x.Category == PowerPriceCategory.Low).Select(x => x.Hour).ToList());
    }

    [Fact]
    public void FindLowestHours_WhenCalled_FindsThreeLowestHours()
    {
        // Arrange
        var powerPrices = new List<PowerPrice>
        {
            new(0, 10),
            new(1, 20),
            new(2, 30),
            new(3, 40),
            new(4, 50),
            new(5, 10),
            new(6, 10),
            new(7, 10),
        };
        var powerPriceSettings = new PowerPriceSettings { NumberOfLowPriceHours = 3 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        powerPriceAggregate.FindLowestHours();

        // Assert
        Assert.Equal(new List<int> { 5, 6, 7 }, powerPriceAggregate.PowerPrices.Where(x => x.Category == PowerPriceCategory.Low).Select(x => x.Hour).ToList());
    }

    [Fact]
    public void FindHighestHours_WhenCalled_FindsTwoHighestHours()
    {
        // Arrange
        var powerPrices = new List<PowerPrice>
        {
            new(0, 10),
            new(1, 20),
            new(2, 30),
            new(3, 40),
            new(4, 50),
            new(5, 10),
            new(6, 10),
            new(7, 10),
            new(8, 50),
            new(9, 60),
        };
        var powerPriceSettings = new PowerPriceSettings { NumberOfHighPriceHours = 2 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        powerPriceAggregate.FindHighestHours();

        // Assert
        Assert.Equal(new List<int> { 8, 9 }, powerPriceAggregate.PowerPrices.Where(x => x.Category == PowerPriceCategory.High).Select(x => x.Hour).ToList());
    }

    [Fact]
    public void FindHighestHours_WhenCalled_FindsThreeHighestHours()
    {
        // Arrange
        var powerPrices = new List<PowerPrice>
        {
            new(0, 10),
            new(1, 20),
            new(2, 30),
            new(3, 40),
            new(4, 50),
            new(5, 10),
            new(6, 10),
            new(7, 10),
        };
        var powerPriceSettings = new PowerPriceSettings { NumberOfHighPriceHours = 3 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        powerPriceAggregate.FindHighestHours();

        // Assert
        Assert.Equal(new List<int> { 2, 3, 4 }, powerPriceAggregate.PowerPrices.Where(x => x.Category == PowerPriceCategory.High).Select(x => x.Hour).ToList());
    }

    [Fact]
    public void Constructor_NoPowerPrices_CreatesCorrectList()
    {
        // Arrange
        var powerPrices = new List<PowerPrice>();
        var powerPriceSettings = new PowerPriceSettings { NumberOfHighPriceHours = 3, NumberOfLowPriceHours = 3 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        var powerPricesFromAggregate = powerPriceAggregate.PowerPrices;

        // Assert
        Assert.Equal(24, powerPricesFromAggregate.Count);
        Assert.Equal(0, powerPricesFromAggregate[0].Hour);
        // Should be reverted to category Low afterwards
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[0].Category);
        Assert.Equal(23, powerPricesFromAggregate[23].Hour);
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[23].Category);
        // Added for this fine scenario
        Assert.True(powerPricesFromAggregate.All(x => x.Category == PowerPriceCategory.Normal));
    }

    [Fact]
    public void Constructor_ZeroNumberOfLowPriceHours_CreatesCorrectList()
    {
        // Arrange
        var powerPrices = Enumerable.Range(0, 24).Select(i => new PowerPrice(i, 0)).ToList();
        var powerPriceSettings = new PowerPriceSettings { NumberOfHighPriceHours = 3, NumberOfLowPriceHours = 0 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        var powerPricesFromAggregate = powerPriceAggregate.PowerPrices;

        // Assert
        Assert.Equal(24, powerPricesFromAggregate.Count);
        Assert.Equal(0, powerPricesFromAggregate[0].Hour);
        // Should be reverted to category Low afterwards
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[0].Category);
        Assert.Equal(23, powerPricesFromAggregate[23].Hour);
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[23].Category);
        // Added for this fine scenario
        Assert.True(powerPricesFromAggregate.All(x => x.Category == PowerPriceCategory.Normal));
    }

    [Fact]
    public void Constructor_ZeroNumberOfHighPriceHours_CreatesCorrectList()
    {
        // Arrange
        var powerPrices = Enumerable.Range(0, 24).Select(i => new PowerPrice(i, 0)).ToList();
        var powerPriceSettings = new PowerPriceSettings { NumberOfHighPriceHours = 0, NumberOfLowPriceHours = 3 };
        var powerPriceAggregate = new PowerPriceAggregate(powerPrices, powerPriceSettings);

        // Act
        var powerPricesFromAggregate = powerPriceAggregate.PowerPrices;

        // Assert
        Assert.Equal(24, powerPricesFromAggregate.Count);
        Assert.Equal(0, powerPricesFromAggregate[0].Hour);
        // Should be reverted to category Low afterwards
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[0].Category);
        Assert.Equal(23, powerPricesFromAggregate[23].Hour);
        Assert.Equal(PowerPriceCategory.Normal, powerPricesFromAggregate[23].Category);
        // Added for this fine scenario
        Assert.True(powerPricesFromAggregate.All(x => x.Category == PowerPriceCategory.Normal));
    }
}