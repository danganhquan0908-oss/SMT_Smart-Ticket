using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartTicket.Infrastructure.Persistence;

public sealed class SmartTicketDbContextFactory : IDesignTimeDbContextFactory<SmartTicketDbContext>
{
    public SmartTicketDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=smartticket;Username=smartticket;Password=smartticket_dev_only";
        var options = new DbContextOptionsBuilder<SmartTicketDbContext>().UseNpgsql(connectionString).Options;
        return new SmartTicketDbContext(options);
    }
}
