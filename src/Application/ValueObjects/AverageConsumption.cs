namespace Application.ValueObjects;

public class AverageConsumption
{
    private static double _maxValue;
    private double _value { get; init; }
    public double Value { get => _value; }

    public AverageConsumption(double value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Average consumption is under 0");
        }
        _value = value;
    }

    public static void SetMaxValue(double maxValue)
    {
        _maxValue = maxValue;
        Console.WriteLine($"AverageConsumption max value set to {maxValue}");
    }

    public bool IsOver()
    {
        return _value >= _maxValue;
    }
}
