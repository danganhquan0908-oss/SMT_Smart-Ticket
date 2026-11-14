namespace BusGoSmartTicket.Exceptions.Bookings;

public sealed class BookingConflictException(string message) : Exception(message);
