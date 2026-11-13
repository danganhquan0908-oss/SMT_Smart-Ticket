namespace SmartTicket.Domain.Entities;

public sealed class TicketScanLog
{
    private TicketScanLog() { }
    public TicketScanLog(Guid ticketId, Guid userId, Guid? stationId, string result, bool offline, DateTime scannedAtUtc)
    { Id = Guid.NewGuid(); TicketId = ticketId; UserId = userId; StationId = stationId; Result = result; IsOffline = offline; ScannedAtUtc = scannedAtUtc; }
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public TripTicket? Ticket { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? StationId { get; private set; }
    public string Result { get; private set; } = "";
    public bool IsOffline { get; private set; }
    public DateTime ScannedAtUtc { get; private set; }
}
