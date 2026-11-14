namespace BusGoSmartTicket.Exceptions.Operations;

public sealed class OperationsConflictException(string message) : Exception(message);
