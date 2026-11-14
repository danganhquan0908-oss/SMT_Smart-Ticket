using System.Security.Claims;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Interfaces.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace BusGoSmartTicket.Controllers;

[Route("account")]
public sealed class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ICustomerAccountService customerAccount) : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpGet("register")]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new PassengerRegistrationViewModel());
    }

    [HttpPost("register"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(PassengerRegistrationViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
            return View(model);
        }

        const string customerRole = "CUSTOMER";
        if (!await roleManager.RoleExistsAsync(customerRole))
        {
            var roleCreated = await roleManager.CreateAsync(new IdentityRole<Guid>(customerRole));
            if (!roleCreated.Succeeded)
            {
                foreach (var error in roleCreated.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = model.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
            IsActive = true
        };
        var created = await userManager.CreateAsync(user, model.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, customerRole);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = "Tạo tài khoản hành khách thành công.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu chưa chính xác.");
            return View(model);
        }
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var isOperator = await signInManager.UserManager.IsInRoleAsync(user, "OPERATOR_OWNER") || await signInManager.UserManager.IsInRoleAsync(user, "OPERATOR_DISPATCHER");
            if (isOperator)
            {
                if (IsOperatorReturnUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl!);
                return RedirectToAction("Index", "OperatorPortal");
            }
            if (await signInManager.UserManager.IsInRoleAsync(user, "STATION_AGENT"))
            {
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.StartsWith("/station", StringComparison.OrdinalIgnoreCase))
                    return LocalRedirect(model.ReturnUrl);
                return RedirectToAction("Index", "StationPortal");
            }
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);
            if (IsAdminReturnUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl!);
            return RedirectToAction("Index", "Home");
        }
        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(TwoFactor), new
            {
                returnUrl = model.ReturnUrl,
                rememberMe = model.RememberMe
            });
        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu chưa chính xác.");
        return View(model);
    }

    [HttpGet("two-factor")]
    public async Task<IActionResult> TwoFactor(string? returnUrl, bool rememberMe)
    {
        if (await signInManager.GetTwoFactorAuthenticationUserAsync() is null)
            return RedirectToAction(nameof(Login));
        return View(new TwoFactorViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    [HttpPost("two-factor"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var pendingUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, rememberClient: false);
        if (result.Succeeded)
        {
            var isOperator = pendingUser is not null && (await signInManager.UserManager.IsInRoleAsync(pendingUser, "OPERATOR_OWNER") || await signInManager.UserManager.IsInRoleAsync(pendingUser, "OPERATOR_DISPATCHER"));
            if (isOperator)
            {
                if (IsOperatorReturnUrl(model.ReturnUrl))
                    return LocalRedirect(model.ReturnUrl!);
                return RedirectToAction("Index", "OperatorPortal");
            }
            if (pendingUser is not null && await signInManager.UserManager.IsInRoleAsync(pendingUser, "STATION_AGENT"))
            {
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.StartsWith("/station", StringComparison.OrdinalIgnoreCase))
                    return LocalRedirect(model.ReturnUrl);
                return RedirectToAction("Index", "StationPortal");
            }
            if (IsAdminReturnUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl!);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(nameof(model.Code), "Mã xác thực không chính xác.");
        return View(model);
    }

    [HttpPost("logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("profile"), Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var model = new CustomerProfileViewModel
        {
            FullName = user.FullName ?? user.UserName ?? "",
            PhoneNumber = user.PhoneNumber,
            Email = user.Email ?? ""
        };
        return View(model);
    }

    [HttpPost("profile"), Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CustomerProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        model.Email = user.Email ?? "";
        if (!ModelState.IsValid)
            return View(model);

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["ProfileSuccess"] = "Cập nhật thông tin cá nhân thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("change-password"), Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        if (!ModelState.IsValid)
        {
            TempData["PasswordError"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Profile));
        }

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            TempData["PasswordError"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Profile));
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("tickets"), Authorize]
    public async Task<IActionResult> Tickets(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Challenge();

        var bookings = await customerAccount.GetBookingsAsync(userId, ct);
        var now = DateTime.UtcNow;

        var upcoming = bookings.Where(b => b.Status == "Paid" && b.DepartureTimeUtc > now).ToList();
        var pending = bookings.Where(b => b.Status == "Pending" && b.ExpiresAtUtc > now).ToList();
        var history = bookings.Where(b => !(b.Status == "Paid" && b.DepartureTimeUtc > now) && !(b.Status == "Pending" && b.ExpiresAtUtc > now)).ToList();

        return View(new CustomerTicketsViewModel
        {
            Upcoming = upcoming,
            Pending = pending,
            History = history
        });
    }

    [HttpGet("tickets/{bookingId:guid}"), Authorize]
    public async Task<IActionResult> TicketDetails(Guid bookingId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Challenge();

        var booking = await customerAccount.GetBookingDetailsAsync(userId, bookingId, ct);
        if (booking is null)
            return NotFound();

        ViewBag.TicketQr = booking.Tickets.ToDictionary(
            x => x.TicketNumber,
            x =>
            {
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(x.CheckInJwt, QRCodeGenerator.ECCLevel.Q);
                return Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(8));
            });

        return View(booking);
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }

    private bool IsOperatorReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) && value.StartsWith("/operator", StringComparison.OrdinalIgnoreCase);
    private bool IsAdminReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) &&
        (value == "/" || value.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/Home", StringComparison.OrdinalIgnoreCase));
}
