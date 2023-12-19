using Application.Aggregates;

namespace Application.Interfaces;

public interface IConsumptionAggregateFactory
{
    ConsumptionAggregate Create(double activeEffect, double averageConsumption);
}