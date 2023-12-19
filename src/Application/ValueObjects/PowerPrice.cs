namespace Application.ValueObjects;

public class PowerPrice
{
    public int Hour { get; private set; }
    public double Value { get; private set; }
    public PowerPriceCategory Category { get; private set; }

    public PowerPrice(int hour, double value)
    {
        SetHour(hour);
        Value = value;
        Category = PowerPriceCategory.Normal;
    }

    public override string ToString()
    {
        return $"Hour: {Hour}, Value: {Value}, Category: {Category}";
    }

    private void SetHour(int hour)
    {
        if (hour < 0 || hour > 23)
        {
            throw new ArgumentException("Hour must be between 0 and 23");
        }
        Hour = hour;
    }

    public void SetCategory(int category)
    {
        if (category < 0 || category > 2)
        {
            throw new ArgumentException("Price category must be between 0 and 2");
        }
        Category = (PowerPriceCategory)category;
    }
}

public enum PowerPriceCategory
{
    Low,
    Normal,
    High
}