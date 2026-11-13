using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Catalog;
using SmartTicket.Application.Operators;
using SmartTicket.Infrastructure.Catalog;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Infrastructure.Operators;
using SmartTicket.Infrastructure.Bookings;
using SmartTicket.Application.Bookings;
using SmartTicket.Web.Hubs;
using SmartTicket.Web;
using SmartTicket.Infrastructure.Payments;
using SmartTicket.Application.Payments;
using SmartTicket.Application.Operations;
using SmartTicket.Infrastructure.Operations;
using SmartTicket.Web.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
var databaseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection.");
var databaseConnection = new Npgsql.NpgsqlConnectionStringBuilder(databaseConnectionString);
var maximumPoolSize = builder.Configuration.GetValue<int?>("Database:MaximumPoolSize");
if (maximumPoolSize is > 0)
    databaseConnection.MaxPoolSize = maximumPoolSize.Value;
builder.Services.AddDbContext<SmartTicketDbContext>(options =>
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
    .AddEntityFrameworkStores<SmartTicketDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
    options.Cookie.Name = "SmartTicket.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")) { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")) { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<ICatalogService, SmartTicket.Application.Catalog.CatalogService>();
builder.Services.AddScoped<ICatalogRepository, EfCatalogRepository>();
builder.Services.AddScoped<ICatalogCache, DistributedCatalogCache>();
builder.Services.AddScoped<IOperatorService, SmartTicket.Application.Operators.OperatorService>();
builder.Services.AddScoped<IOperatorRepository, EfOperatorRepository>();
builder.Services.AddScoped<IPrivateLicenseStore, PrivateLicenseStore>();
builder.Services.AddScoped<IActivationEmailSender, SmtpActivationEmailSender>();
builder.Services.AddSingleton<ISeatLockManager, DistributedSeatLockManager>();
builder.Services.AddScoped<IBookingService, EfBookingService>();
builder.Services.AddScoped<IPaymentService, EfPaymentService>();
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
        Console.WriteLine($"[Cảnh báo] Không thể kết nối Redis ({redisConnection}). Tự động fallback sang In-Memory Cache.");
    }
    builder.Services.AddDistributedMemoryCache();
}

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
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
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<TripSeatsHub>("/hubs/trip-seats");

await InitializeDatabaseAsync(app);
app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<SmartTicketDbContext>();
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
        if (!user.IsActive) { user.IsActive = true; await userManager.UpdateAsync(user); }
        if (!await userManager.IsInRoleAsync(user, "SUPER_ADMIN"))
            await userManager.AddToRoleAsync(user, "SUPER_ADMIN");
    }

    if (app.Environment.IsDevelopment())
        await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

public partial class Program;
