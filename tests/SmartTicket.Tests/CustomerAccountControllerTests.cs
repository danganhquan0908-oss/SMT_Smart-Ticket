using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SmartTicket.Application.Account;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Web.Controllers;
using SmartTicket.Web.Models;

namespace SmartTicket.Tests;

public sealed class CustomerAccountControllerTests
{
    [Fact]
    public async Task Tickets_CategorizesUpcomingPendingAndHistory()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var upcoming = new CustomerBookingSummaryDto(
            Guid.NewGuid(), "UP123456", "Paid", 200000m, now.AddHours(-1), now.AddMinutes(6),
            "Bến Miền Đông", "Bến Đà Lạt", "SMT Express", "Limousine",
            now.AddDays(2), ["A01"], []);

        var pending = new CustomerBookingSummaryDto(
            Guid.NewGuid(), "PEN12345", "Pending", 150000m, now.AddMinutes(-2), now.AddMinutes(5),
            "Bến Miền Đông", "Bến Nha Trang", "SMT Express", "Giường nằm",
            now.AddDays(1), ["B02"], []);

        var history = new CustomerBookingSummaryDto(
            Guid.NewGuid(), "HIS12345", "Paid", 180000m, now.AddDays(-5), now.AddDays(-5).AddMinutes(7),
            "Bến Miền Đông", "Bến Cần Thơ", "SMT Express", "Ghế ngồi",
            now.AddDays(-4), ["A05"], []);

        var fakeService = new FakeCustomerAccountService([upcoming, pending, history]);
        var controller = CreateController(fakeService, userId);

        var actionResult = await controller.Tickets(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(actionResult);
        var model = Assert.IsType<CustomerTicketsViewModel>(viewResult.Model);

        Assert.Single(model.Upcoming);
        Assert.Equal("UP123456", model.Upcoming[0].BookingCode);

        Assert.Single(model.Pending);
        Assert.Equal("PEN12345", model.Pending[0].BookingCode);

        Assert.Single(model.History);
        Assert.Equal("HIS12345", model.History[0].BookingCode);
    }

    [Fact]
    public async Task TicketDetails_WhenBookingFound_GeneratesTicketQrAndReturnsView()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMifQ.signature";

        var booking = new CustomerBookingSummaryDto(
            bookingId, "TK998877", "Paid", 250000m, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow,
            "Bến Miền Đông", "Bến Đà Lạt", "SMT Express", "Limousine",
            DateTime.UtcNow.AddDays(1), ["A01"],
            [new CustomerTicketDto(Guid.NewGuid(), "TCK-1", "A01", 250000m, DateTime.UtcNow, "Sold", jwt)]);

        var fakeService = new FakeCustomerAccountService([booking]);
        var controller = CreateController(fakeService, userId);

        var actionResult = await controller.TicketDetails(bookingId, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(actionResult);
        var model = Assert.IsType<CustomerBookingSummaryDto>(viewResult.Model);
        Assert.Equal(bookingId, model.BookingId);

        var qrDict = Assert.IsType<Dictionary<string, string>>(viewResult.ViewData["TicketQr"]);
        Assert.True(qrDict.ContainsKey("TCK-1"));
        Assert.False(string.IsNullOrWhiteSpace(qrDict["TCK-1"]));
    }

    [Fact]
    public async Task TicketDetails_WhenBookingNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var fakeService = new FakeCustomerAccountService([]);
        var controller = CreateController(fakeService, userId);

        var actionResult = await controller.TicketDetails(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult);
    }

    private static AccountController CreateController(ICustomerAccountService customerAccount, Guid userId)
    {
        var userStore = new FakeUserStore();
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new HttpContextAccessor();
        var claimsFactory = new UserClaimsPrincipalFactory<ApplicationUser>(userManager, Microsoft.Extensions.Options.Options.Create(new IdentityOptions()));
        var signInManager = new SignInManager<ApplicationUser>(
            userManager,
            contextAccessor,
            claimsFactory,
            null!, null!, null!, null!);

        var roleStore = new FakeRoleStore();
        var roleManager = new RoleManager<IdentityRole<Guid>>(
            roleStore, null!, null!, null!, null!);

        var controller = new AccountController(signInManager, userManager, roleManager, customerAccount);
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "khachhang@smartticket.vn")
        ], "TestAuth");

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());

        return controller;
    }

    private sealed class FakeCustomerAccountService(IReadOnlyList<CustomerBookingSummaryDto> bookings) : ICustomerAccountService
    {
        public Task<CustomerProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<CustomerProfileDto?>(new CustomerProfileDto(userId, "Đặng Anh Quân", "khachhang@smartticket.vn", "0345588495"));

        public Task<IReadOnlyList<CustomerBookingSummaryDto>> GetBookingsAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(bookings);

        public Task<CustomerBookingSummaryDto?> GetBookingDetailsAsync(Guid userId, Guid bookingId, CancellationToken ct = default) =>
            Task.FromResult(bookings.FirstOrDefault(x => x.BookingId == bookingId));
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        private readonly Dictionary<string, object> _data = new();
        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class FakeUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id.ToString());
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) { user.UserName = userName; return Task.CompletedTask; }
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) { user.NormalizedUserName = normalizedName; return Task.CompletedTask; }
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
    }

    private sealed class FakeRoleStore : IRoleStore<IdentityRole<Guid>>
    {
        public void Dispose() { }
        public Task<IdentityResult> CreateAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<string> GetRoleIdAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(role.Id.ToString());
        public Task<string?> GetRoleNameAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(role.Name);
        public Task SetRoleNameAsync(IdentityRole<Guid> role, string? roleName, CancellationToken ct) { role.Name = roleName; return Task.CompletedTask; }
        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole<Guid> role, CancellationToken ct) => Task.FromResult(role.NormalizedName);
        public Task SetNormalizedRoleNameAsync(IdentityRole<Guid> role, string? normalizedName, CancellationToken ct) { role.NormalizedName = normalizedName; return Task.CompletedTask; }
        public Task<IdentityRole<Guid>?> FindByIdAsync(string roleId, CancellationToken ct) => Task.FromResult<IdentityRole<Guid>?>(null);
        public Task<IdentityRole<Guid>?> FindByNameAsync(string normalizedRoleName, CancellationToken ct) => Task.FromResult<IdentityRole<Guid>?>(null);
    }
}
