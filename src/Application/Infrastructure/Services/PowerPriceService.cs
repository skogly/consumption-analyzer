using Application.Interfaces;
using Application.ValueObjects;

namespace Application.Infrastructure.Services;

public class PowerPriceService : IPowerPriceService
{
    private readonly List<PowerPrice> _powerPrices;

    public PowerPriceService()
    {
        _powerPrices = new();
    }

    public void UpdatePowerPrices(IReadOnlyList<PowerPrice> powerPrices)
    {
        _powerPrices.Clear();
        powerPrices.Where(x => x.Category == PowerPriceCategory.Low).ToList().ForEach(x => System.Console.WriteLine(x.ToString()));
        if (!powerPrices.Any(x => x.Category == PowerPriceCategory.Low))
        {
            System.Console.WriteLine("No low power prices");
        }
        powerPrices.Where(x => x.Category == PowerPriceCategory.High).ToList().ForEach(x => System.Console.WriteLine(x.ToString()));
        if (!powerPrices.Any(x => x.Category == PowerPriceCategory.High))
        {
            System.Console.WriteLine("No high power prices");
        }
        _powerPrices.AddRange(powerPrices);
    }

    public PowerPriceCategory GetCurrentPowerPriceCategory()
    {
        var powerPrice = _powerPrices.FirstOrDefault(x => x.Hour == DateTime.Now.ToLocalTime().Hour);

        if (powerPrice is null)
        {
            return PowerPriceCategory.Normal;
        }

        return powerPrice.Category;
    }

    public IReadOnlyList<PowerPrice> PowerPrices()
    {
        return _powerPrices;
    }

    public void SetAllPowerPricesToNormal()
    {
        _powerPrices.ForEach(x => x.SetCategory(0));
    }
}