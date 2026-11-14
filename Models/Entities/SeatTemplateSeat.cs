namespace BusGoSmartTicket.Models.Entities;

public sealed class SeatTemplateSeat
{
    private SeatTemplateSeat()
    {
    }
    public SeatTemplateSeat(Guid templateId, int deck, int number, string code, int row, int column)
    {
        Id = Guid.NewGuid();
        SeatTemplateId = templateId;
        Deck = deck;
        Number = number;
        SeatCode = code;
        Row = row;
        Column = column;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid SeatTemplateId
    {
        get; private set;
    }
    public SeatTemplate? SeatTemplate
    {
        get; private set;
    }
    public int Deck
    {
        get; private set;
    }
    public int Number
    {
        get; private set;
    }
    public string SeatCode { get; private set; } = string.Empty;
    public int Row
    {
        get; private set;
    }
    public int Column
    {
        get; private set;
    }
}
