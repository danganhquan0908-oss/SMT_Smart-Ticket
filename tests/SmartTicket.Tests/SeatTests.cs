using SmartTicket.Domain.Entities;

namespace SmartTicket.Tests;

public sealed class SeatTests
{
    private static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TryHold_AvailableSeat_BecomesHeldByBooking()
    {
        var bookingId = Guid.NewGuid();
        var seat = CreateSeat();

        var held = seat.TryHold(bookingId, Now, TimeSpan.FromMinutes(7));

        Assert.True(held);
        Assert.Equal(SeatStatus.Holding, seat.Status);
        Assert.Equal(bookingId, seat.CurrentBookingId);
        Assert.Equal(Now.AddMinutes(7), seat.LockedUntilUtc);
    }

    [Fact]
    public void TryHold_ActiveHold_CannotBeTakenByAnotherBooking()
    {
        var seat = CreateSeat();
        var firstBooking = Guid.NewGuid();

        Assert.True(seat.TryHold(firstBooking, Now, TimeSpan.FromMinutes(7)));
        Assert.False(seat.TryHold(Guid.NewGuid(), Now.AddMinutes(1), TimeSpan.FromMinutes(7)));
        Assert.Equal(firstBooking, seat.CurrentBookingId);
    }

    [Fact]
    public void TryHold_ExpiredHold_CanBeReclaimed()
    {
        var seat = CreateSeat();
        var nextBooking = Guid.NewGuid();
        Assert.True(seat.TryHold(Guid.NewGuid(), Now, TimeSpan.FromMinutes(7)));

        Assert.True(seat.TryHold(nextBooking, Now.AddMinutes(7), TimeSpan.FromMinutes(7)));
        Assert.Equal(nextBooking, seat.CurrentBookingId);
    }

    [Fact]
    public void ReleaseHold_OnlyOwnerCanReleaseIt()
    {
        var seat = CreateSeat();
        var owner = Guid.NewGuid();
        Assert.True(seat.TryHold(owner, Now, TimeSpan.FromMinutes(7)));

        seat.ReleaseHold(Guid.NewGuid());
        Assert.Equal(SeatStatus.Holding, seat.Status);

        seat.ReleaseHold(owner);
        Assert.Equal(SeatStatus.Available, seat.Status);
        Assert.Null(seat.CurrentBookingId);
        Assert.Null(seat.LockedUntilUtc);
    }

    [Fact]
    public void TryHold_BookedSeat_IsRejected()
    {
        var seat = CreateSeat();
        var bookingId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        Assert.True(seat.TryHold(bookingId, now, TimeSpan.FromMinutes(7)));
        seat.ConfirmBooking(bookingId);

        Assert.False(seat.TryHold(Guid.NewGuid(), now.AddDays(1), TimeSpan.FromMinutes(7)));
    }

    private static Seat CreateSeat() => new(Guid.NewGuid(), "A01", 1, 1, 1);
}
