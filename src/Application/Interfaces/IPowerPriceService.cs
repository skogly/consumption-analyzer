using Application.ValueObjects;

namespace Application.Interfaces;

public interface IPowerPriceService
{
    void UpdatePowerPrices(IReadOnlyList<PowerPrice> powerPrices);

    PowerPriceCategory GetCurrentPowerPriceCategory();

    IReadOnlyList<PowerPrice> PowerPrices();

    void SetAllPowerPricesToNormal();
}