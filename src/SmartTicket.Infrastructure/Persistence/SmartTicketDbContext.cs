using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Domain.Entities;

namespace SmartTicket.Infrastructure.Persistence;

public sealed class SmartTicketDbContext(DbContextOptions<SmartTicketDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<BusRoute> Routes => Set<BusRoute>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<BusOperator> BusOperators => Set<BusOperator>();
    public DbSet<OperatorMembership> OperatorMemberships => Set<OperatorMembership>();
    public DbSet<SeatTemplate> SeatTemplates => Set<SeatTemplate>();
    public DbSet<SeatTemplateSeat> SeatTemplateSeats => Set<SeatTemplateSeat>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<TripAuditLog> TripAuditLogs => Set<TripAuditLog>();
    public DbSet<TripTicket> TripTickets => Set<TripTicket>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StationMembership> StationMemberships => Set<StationMembership>();
    public DbSet<DispatchClearance> DispatchClearances => Set<DispatchClearance>();
    public DbSet<TicketScanLog> TicketScanLogs => Set<TicketScanLog>();
    public DbSet<TripSettlement> TripSettlements => Set<TripSettlement>();
    public DbSet<OperatorActivation> OperatorActivations => Set<OperatorActivation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Station>(entity =>
        {
            entity.ToTable("Stations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(12).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Province).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
            entity.HasQueryFilter(x => x.IsActive);
        });

        builder.Entity<BusRoute>(entity =>
        {
            entity.ToTable("Routes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DistanceKm).HasPrecision(8, 2);
            entity.HasIndex(x => new { x.DepartureStationId, x.ArrivalStationId }).IsUnique();
            entity.HasOne(x => x.DepartureStation).WithMany().HasForeignKey(x => x.DepartureStationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ArrivalStation).WithMany().HasForeignKey(x => x.ArrivalStationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => x.IsActive);
        });

        builder.Entity<Trip>(entity =>
        {
            entity.ToTable("Trips");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Route).WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.TicketPrice).HasPrecision(12, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasMany(x => x.Seats).WithOne(x => x.Trip).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Seat>(entity =>
        {
            entity.ToTable("Seats"); entity.HasKey(x => x.Id);
            entity.Property(x => x.SeatCode).HasMaxLength(12).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => new { x.TripId, x.SeatCode }).IsUnique();
            entity.HasIndex(x => new { x.Status, x.LockedUntilUtc });
        });
        builder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings"); entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.BookingCode).IsUnique();
            entity.Property(x => x.BookingCode).HasMaxLength(16).IsRequired();
            entity.Property(x => x.PassengerName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PassengerPhone).HasMaxLength(24).IsRequired();
            entity.Property(x => x.PassengerEmail).HasMaxLength(254);
            entity.Property(x => x.TotalAmount).HasPrecision(12, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.LookupTokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.LookupTokenHash).IsUnique();
            entity.HasMany(x => x.Seats).WithOne(x => x.Booking).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Trip).WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<BookingSeat>(entity =>
        {
            entity.ToTable("BookingSeats"); entity.HasKey(x => x.Id);
            entity.Property(x => x.SeatCode).HasMaxLength(12).IsRequired();
            entity.Property(x => x.PriceAtPurchase).HasPrecision(12, 2);
            entity.HasIndex(x => new { x.BookingId, x.SeatId }).IsUnique();
            entity.HasOne<Seat>().WithMany().HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUser>().Property(x => x.IsActive).HasDefaultValue(false);
        builder.Entity<BusOperator>(entity =>
        {
            entity.ToTable("BusOperators");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TaxCode).IsUnique();
            entity.Property(x => x.CompanyName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.TaxCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Hotline).HasMaxLength(24).IsRequired();
            entity.Property(x => x.BusinessLicensePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.PayoutBankBin).HasMaxLength(6);
            entity.Property(x => x.PayoutAccountNumber).HasMaxLength(28);
            entity.Property(x => x.PayoutAccountName).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<OperatorMembership>(entity =>
        {
            entity.ToTable("OperatorMemberships");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OperatorId, x.UserId }).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(24);
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<SeatTemplate>(entity =>
        {
            entity.ToTable("SeatTemplates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => new { x.OperatorId, x.Name }).IsUnique();
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Seats).WithOne(x => x.SeatTemplate).HasForeignKey(x => x.SeatTemplateId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<SeatTemplateSeat>(entity =>
        {
            entity.ToTable("SeatTemplateSeats");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.SeatTemplateId, x.SeatCode }).IsUnique();
            entity.Property(x => x.SeatCode).HasMaxLength(12).IsRequired();
        });
        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OperatorId, x.PlateNumber }).IsUnique();
            entity.Property(x => x.PlateNumber).HasMaxLength(16).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SeatTemplate).WithMany().HasForeignKey(x => x.SeatTemplateId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Driver>(entity =>
        {
            entity.ToTable("Drivers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OperatorId, x.LicenseNumber }).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(24).IsRequired();
            entity.Property(x => x.LicenseNumber).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<TripAuditLog>(entity =>
        {
            entity.ToTable("TripAuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OldPrice).HasPrecision(12, 2);
            entity.Property(x => x.NewPrice).HasPrecision(12, 2);
            entity.Property(x => x.Action).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.Trip).WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ModifiedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<TripTicket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PriceAtPurchase).HasPrecision(12, 2);
            entity.Property(x => x.SeatCode).HasMaxLength(12).IsRequired();
            entity.Property(x => x.CheckInToken).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CheckedInByUserId);
            entity.HasOne(x => x.Trip).WithMany(x => x.Tickets).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Booking>().WithMany().HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Seat>().WithMany().HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TripId, x.Status });
            entity.HasIndex(x => x.SeatId).IsUnique();
        });
        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Reference).HasMaxLength(25).IsRequired();
            entity.Property(x => x.ProviderTransactionId).HasMaxLength(120);
            entity.Property(x => x.Amount).HasPrecision(12, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.RawWebhookPayload).HasColumnType("text");
            entity.HasIndex(x => x.Reference).IsUnique();
            entity.HasIndex(x => x.ProviderTransactionId).IsUnique();
            entity.HasOne(x => x.Booking).WithOne().HasForeignKey<Payment>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<StationMembership>(entity =>
        {
            entity.ToTable("StationMemberships"); entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.StationId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Station).WithMany().HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<DispatchClearance>(entity =>
        {
            entity.ToTable("DispatchClearances"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Signature).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.TripId, x.CreatedAtUtc });
            entity.HasOne(x => x.Trip).WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<TicketScanLog>(entity =>
        {
            entity.ToTable("TicketScanLogs"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TicketId, x.ScannedAtUtc });
            entity.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<TripSettlement>(entity =>
        {
            entity.ToTable("TripSettlements"); entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TripId).IsUnique();
            entity.Property(x => x.GrossAmount).HasPrecision(12, 2);
            entity.Property(x => x.CommissionAmount).HasPrecision(12, 2);
            entity.Property(x => x.NetAmount).HasPrecision(12, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => new { x.Status, x.ProcessingAtUtc });
            entity.Property(x => x.ProviderReference).HasMaxLength(160);
            entity.Property(x => x.FailureReason).HasMaxLength(500);
            entity.HasOne(x => x.Trip).WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<OperatorActivation>(entity =>
        {
            entity.ToTable("OperatorActivations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
