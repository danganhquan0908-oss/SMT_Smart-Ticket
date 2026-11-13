namespace SmartTicket.Domain.Entities;

public sealed class SeatTemplate
{
    private SeatTemplate() { }
    public SeatTemplate(Guid operatorId, string name, string vehicleType, int deckCount, int seatsPerDeck)
    {
        if (deckCount is < 1 or > 2 || seatsPerDeck is < 1 or > 60) throw new ArgumentOutOfRangeException(nameof(seatsPerDeck));
        Id = Guid.NewGuid(); OperatorId = operatorId; Name = name.Trim(); VehicleType = vehicleType.Trim();
        DeckCount = deckCount; SeatsPerDeck = seatsPerDeck; IsActive = true; CreatedAtUtc = DateTime.UtcNow;
        Seats = Enumerable.Range(1, deckCount).SelectMany(deck => Enumerable.Range(1, seatsPerDeck)
            .Select(number => new SeatTemplateSeat(Id, deck, number, $"{(char)('A' + deck - 1)}{number:00}", ((number - 1) / 4) + 1, new[] { 1, 2, 4, 5 }[(number - 1) % 4]))).ToList();
    }
    public Guid Id { get; private set; }
    public Guid OperatorId { get; private set; }
    public BusOperator? Operator { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string VehicleType { get; private set; } = string.Empty;
    public int DeckCount { get; private set; }
    public int SeatsPerDeck { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public ICollection<SeatTemplateSeat> Seats { get; private set; } = new List<SeatTemplateSeat>();
    public void Deactivate() => IsActive = false;
}

public sealed class SeatTemplateSeat
{
    private SeatTemplateSeat() { }
    public SeatTemplateSeat(Guid templateId, int deck, int number, string code, int row, int column)
    { Id = Guid.NewGuid(); SeatTemplateId = templateId; Deck = deck; Number = number; SeatCode = code; Row = row; Column = column; }
    public Guid Id { get; private set; }
    public Guid SeatTemplateId { get; private set; }
    public SeatTemplate? SeatTemplate { get; private set; }
    public int Deck { get; private set; }
    public int Number { get; private set; }
    public string SeatCode { get; private set; } = string.Empty;
    public int Row { get; private set; }
    public int Column { get; private set; }
}
