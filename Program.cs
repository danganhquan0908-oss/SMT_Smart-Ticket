using BusGoSmartTicket.Data;
using BusGoSmartTicket.Data.Repositories.Catalog;
using BusGoSmartTicket.Data.Repositories.Operators;
using BusGoSmartTicket.Hubs;
using BusGoSmartTicket.Middleware;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Account;
using BusGoSmartTicket.Services.Bookings;
using BusGoSmartTicket.Services.Catalog;
using BusGoSmartTicket.Services.Interfaces.Account;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using BusGoSmartTicket.Services.Interfaces.Catalog;
using BusGoSmartTicket.Services.Interfaces.Operations;
using BusGoSmartTicket.Services.Interfaces.Operators;
using BusGoSmartTicket.Services.Interfaces.Payments;
using BusGoSmartTicket.Services.Operations;
using BusGoSmartTicket.Services.Operators;
using BusGoSmartTicket.Services.Payments;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false).AddEnvironmentVariables();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
var databaseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection.");
var databaseConnection = new Npgsql.NpgsqlConnectionStringBuilder(databaseConnectionString);
var maximumPoolSize = builder.Configuration.GetValue<int?>("Database:MaximumPoolSize");
if (maximumPoolSize is > 0)
    databaseConnection.MaxPoolSize = maximumPoolSize.Value;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(databaseConnection.ConnectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
    options.Cookie.Name = "SmartTicket.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<ICatalogService, BusGoSmartTicket.Services.Catalog.CatalogService>();
builder.Services.AddScoped<ICatalogRepository, EfCatalogRepository>();
builder.Services.AddScoped<ICatalogCache, DistributedCatalogCache>();
builder.Services.AddScoped<IOperatorService, BusGoSmartTicket.Services.Operators.OperatorService>();
builder.Services.AddScoped<IOperatorRepository, EfOperatorRepository>();
builder.Services.AddScoped<IPrivateLicenseStore, PrivateLicenseStore>();
builder.Services.AddScoped<IActivationEmailSender, SmtpActivationEmailSender>();
builder.Services.AddSingleton<ISeatLockManager, DistributedSeatLockManager>();
builder.Services.AddScoped<IBookingService, EfBookingService>();
builder.Services.AddScoped<IPaymentService, EfPaymentService>();
builder.Services.AddScoped<ICustomerAccountService, EfCustomerAccountService>();
builder.Services.AddScoped<IMembershipService, EfMembershipService>();

builder.Services.AddScoped<IStationOperationsService, EfStationOperationsService>();
builder.Services.AddHttpClient<ISettlementPayoutProvider, HttpSettlementPayoutProvider>();
builder.Services.AddHostedService<SettlementWorker>();
builder.Services.AddSingleton<ISeatStatusNotifier, SignalRSeatStatusNotifier>();
builder.Services.AddHostedService<BookingExpirationWorker>();

var redisConnection = builder.Configuration.GetConnectionString("Redis");
var isRedisAvailable = false;
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    try
    {
        var config = ConfigurationOptions.Parse(redisConnection);
        config.ConnectTimeout = 1000;
        config.SyncTimeout = 1000;
        config.AbortOnConnectFail = true;
        using var testConn = ConnectionMultiplexer.Connect(config);
        isRedisAvailable = testConn.IsConnected;
    }
    catch
    {
        isRedisAvailable = false;
    }
}

if (isRedisAvailable)
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
}
else
{
    if (!string.IsNullOrWhiteSpace(redisConnection))
    {
        Console.WriteLine("[Cảnh báo] Redis chưa khả dụng. Cache cục bộ; giữ ghế Production vẫn yêu cầu Redis.");
    }
    builder.Services.AddDistributedMemoryCache();
}

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
foreach (var address in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
{
    if (!System.Net.IPAddress.TryParse(address, out var proxy))
        throw new InvalidOperationException("Địa chỉ reverse proxy không hợp lệ.");
    forwardedHeadersOptions.KnownProxies.Add(proxy);
}
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/api/payments/webhook")
        && !ctx.Request.Path.StartsWithSegments("/api/payments/sepay-webhook"),
    branch => branch.UseHttpsRedirection());
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    if (context.Request.Path.StartsWithSegments("/account") || context.Request.Path.StartsWithSegments("/membership") ||
        context.Request.Path.StartsWithSegments("/tickets") || context.Request.Path.StartsWithSegments("/payments"))
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
    }
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<TripSeatsHub>("/hubs/trip-seats");
app.MapGet("/health", async (ApplicationDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new
    {
        application = "BusGo SmartTicket",
        status = "ready"
    }) : Results.StatusCode(503));

await InitializeDatabaseAsync(app);
app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    if (!await roleManager.RoleExistsAsync("SUPER_ADMIN"))
        await roleManager.CreateAsync(new IdentityRole<Guid>("SUPER_ADMIN"));

    var email = app.Configuration["Admin:Email"];
    var password = app.Configuration["Admin:Password"];
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        app.Logger.LogWarning("SUPER_ADMIN chưa được khởi tạo. Cấu hình Admin:Email và Admin:Password để tạo tài khoản quản trị đầu tiên.");
    else
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true, IsActive = true };
            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new InvalidOperationException("Không tạo được tài khoản Super Admin: " + string.Join("; ", created.Errors.Select(e => e.Description)));
        }
        if (!user.IsActive)
        {
            user.IsActive = true;
            await userManager.UpdateAsync(user);
        }
        if (!await userManager.IsInRoleAsync(user, "SUPER_ADMIN"))
            await userManager.AddToRoleAsync(user, "SUPER_ADMIN");
    }

}

public partial class Program;
