using Application.Settings;
using Application.ValueObjects;

namespace Application.Aggregates;

public class PowerPriceAggregate
{
    private readonly DateTime _date;
    private readonly List<PowerPrice> _powerPrices;
    private readonly PowerPriceSettings _powerPriceSettings;
    public IReadOnlyList<PowerPrice> PowerPrices => _powerPrices;

    public PowerPriceAggregate(List<PowerPrice> powerPrices, PowerPriceSettings powerPriceSettings)
    {
        _date = DateTime.Now.ToLocalTime();
        _powerPriceSettings = powerPriceSettings;
        _powerPrices = SetPowerPrices(powerPrices);
    }

    private List<PowerPrice> SetPowerPrices(List<PowerPrice> powerPrices)
    {
        if (!powerPrices.Any())
        {
            powerPrices = Enumerable.Range(0, 24).Select(i => new PowerPrice(i, 0)).ToList();
        }
        if (_powerPriceSettings.NumberOfLowPriceHours > 0)
        {
            foreach (var price in powerPrices.Where(x => x.Hour < _powerPriceSettings.NumberOfLowPriceHours))
            {
                price.SetCategory(1);
            }
        }
        else
        {
            foreach (var price in powerPrices)
            {
                price.SetCategory(1);
            }
        }

        return powerPrices;
    }

    public void FindLowestHours()
    {
        if (_powerPriceSettings.NumberOfLowPriceHours == 0)
        {
            return;
        }

        var prices = _powerPrices.Select(x => x.Value).ToList();
        if (prices is null)
        {
            return;
        }

        var numHours = _powerPriceSettings.NumberOfLowPriceHours;
        var minAvgPriceByNumHours = FindMinMaxAvgPriceByNumHours(prices, numHours, true);
        SetCategoryForHours(minAvgPriceByNumHours, 0);
    }

    public void FindHighestHours()
    {
        if (_powerPriceSettings.NumberOfHighPriceHours == 0)
        {
            return;
        }

        var prices = _powerPrices.Select(x => x.Value).ToList();
        if (prices is null)
        {
            return;
        }

        var numHours = _powerPriceSettings.NumberOfHighPriceHours;
        var maxAvgPriceByNumHours = FindMinMaxAvgPriceByNumHours(prices, numHours, false);
        SetCategoryForHours(maxAvgPriceByNumHours, 2);
    }

    private KeyValuePair<int, int> FindMinMaxAvgPriceByNumHours(List<double> prices, int numHours, bool findMin)
    {
        var currentSum = prices.Take(numHours).Sum();
        var minMaxAvg = currentSum / numHours;
        var minMaxStartIndex = 0;

        for (int i = numHours; i < prices.Count; i++)
        {
            currentSum = currentSum - prices[i - numHours] + prices[i];
            var currentAvg = currentSum / numHours;

            if (findMin ? currentAvg < minMaxAvg : currentAvg > minMaxAvg)
            {
                minMaxAvg = currentAvg;
                minMaxStartIndex = i - numHours + 1;
            }
        }

        return new KeyValuePair<int, int>(minMaxStartIndex, numHours);
    }

    private void SetCategoryForHours(KeyValuePair<int, int> avgPriceByNumHours, int category)
    {
        var startIndex = avgPriceByNumHours.Key;
        var numHoursToTake = avgPriceByNumHours.Value;

        if (startIndex + numHoursToTake > _powerPrices.Count)
        {
            numHoursToTake = _powerPrices.Count - startIndex;
        }

        var hours = _powerPrices.GetRange(startIndex, numHoursToTake).Select(x => x.Hour).ToList();
        _powerPrices.Where(x => hours.Contains(x.Hour)).ToList().ForEach(x => x.SetCategory(category));
    }
}