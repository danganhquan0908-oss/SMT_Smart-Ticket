namespace BusGoSmartTicket.Exceptions.Payments;

public sealed class PaymentConflictException(string message) : Exception(message);
