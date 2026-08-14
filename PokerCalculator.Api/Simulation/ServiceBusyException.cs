namespace PokerCalculator.Api.Simulation;

// Thrown by EquityService when the admission queue is full; the endpoint maps this to 503.
public class ServiceBusyException : Exception
{
}
