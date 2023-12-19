namespace Application.ValueObjects;

public class ActiveEffect
{
    private static double _maxValue;
    private double _value { get; init; }
    public double Value { get => _value; }

    public ActiveEffect(double value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Active effect value under 0");
        }
        _value = value;
    }

    public static void SetMaxValue(double maxValue)
    {
        _maxValue = maxValue;
        Console.WriteLine($"ActiveEffect max value set to {maxValue}");
    }

    public bool IsOver()
    {
        return _value >= _maxValue;
    }

    public bool IsOver(double value)
    {
        return _value + value >= _maxValue;
    }
}
