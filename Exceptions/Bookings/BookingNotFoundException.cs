namespace BusGoSmartTicket.Exceptions.Bookings;

public sealed class BookingNotFoundException(string message) : Exception(message);
