namespace BusGoSmartTicket.Exceptions.Payments;

public sealed class PaymentNotFoundException(string message) : Exception(message);
