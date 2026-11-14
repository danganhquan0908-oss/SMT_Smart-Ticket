namespace BusGoSmartTicket.Models.Entities;

public sealed class Station
{
    private Station()
    {
    }

    public Station(string code, string name, string province, string address)
    {
        Id = Guid.NewGuid();
        SetDetails(code, name, province, address);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id
    {
        get; private set;
    }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsActive
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime? UpdatedAtUtc
    {
        get; private set;
    }

    public void Update(string code, string name, string province, string address)
    {
        SetDetails(code, name, province, address);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetDetails(string code, string name, string province, string address)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Province = province.Trim();
        Address = address.Trim();
    }
}
