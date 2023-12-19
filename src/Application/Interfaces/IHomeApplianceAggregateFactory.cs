using Application.Aggregates;

namespace Application.Interfaces;

public interface IHomeApplianceAggregateFactory
{
    List<HomeApplianceAggregate> HomeAppliances();
}