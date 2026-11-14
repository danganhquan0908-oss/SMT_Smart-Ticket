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
            if (IsAccountReturnUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl!);
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
            if (IsAccountReturnUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl!);
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

    [HttpGet(""), Authorize]
    public IActionResult Index() => RedirectToAction(nameof(Profile));

    [HttpGet("profile"), Authorize]
    public async Task<IActionResult> Profile(string? section = null)
    {
        var user = await GetActiveUserAsync();
        if (user is null)
            return Challenge();

        return View(await BuildAccountSettingsAsync(user, activeSection: section));
    }

    [HttpPost("profile"), Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile([Bind("FullName,PhoneNumber")] CustomerProfileViewModel model)
    {
        var user = await GetActiveUserAsync();
        if (user is null)
            return Challenge();

        // Identity and permissions are always read from the authenticated user.
        // Remove ignored posted values so Razor cannot redisplay a spoofed email.
        model.Email = user.Email ?? "";
        ModelState.Remove(nameof(CustomerProfileViewModel.Email));
        model.FullName = model.FullName?.Trim() ?? "";
        model.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        if (string.IsNullOrWhiteSpace(model.FullName) &&
            (!ModelState.TryGetValue(nameof(model.FullName), out var fullNameState) || fullNameState.Errors.Count == 0))
            ModelState.AddModelError(nameof(model.FullName), "Vui lòng nhập họ và tên.");
        if (!ModelState.IsValid)
            return View(await BuildAccountSettingsAsync(user, model));

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, ProfileErrorMessage(error));
            return View(await BuildAccountSettingsAsync(user, model));
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["ProfileSuccess"] = "Cập nhật thông tin cá nhân thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("change-password"), Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await GetActiveUserAsync();
        if (user is null)
            return Challenge();

        if (ModelState.IsValid && string.Equals(model.CurrentPassword, model.NewPassword, StringComparison.Ordinal))
            ModelState.AddModelError(nameof(model.NewPassword), "Mật khẩu mới phải khác mật khẩu hiện tại.");
        if (!ModelState.IsValid)
        {
            ClearPasswordValues();
            return View("Profile", await BuildAccountSettingsAsync(user, activeSection: "security"));
        }

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                var field = error.Code == nameof(IdentityErrorDescriber.PasswordMismatch)
                    ? nameof(model.CurrentPassword)
                    : error.Code.StartsWith("Password", StringComparison.Ordinal) ? nameof(model.NewPassword) : "";
                ModelState.AddModelError(field, PasswordErrorMessage(error));
            }
            ClearPasswordValues();
            return View("Profile", await BuildAccountSettingsAsync(user, activeSection: "security"));
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile), new { section = "security" });
    }

    private async Task<AccountSettingsViewModel> BuildAccountSettingsAsync(
        ApplicationUser user, CustomerProfileViewModel? profile = null, string? activeSection = null)
    {
        var roles = await userManager.GetRolesAsync(user);
        var roleLabels = new (string Role, string Label)[]
        {
            ("SUPER_ADMIN", "Quản trị hệ thống"),
            ("OPERATOR_OWNER", "Chủ nhà xe"),
            ("OPERATOR_DISPATCHER", "Điều phối viên"),
            ("STATION_AGENT", "Nhân viên bến"),
            ("CUSTOMER", "Hành khách")
        };
        var labels = roleLabels.Where(item => roles.Contains(item.Role)).Select(item => item.Label).ToArray();
        var displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "" : user.FullName.Trim();
        var nameParts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Concat(nameParts.Take(2).Select(part => System.Globalization.StringInfo.GetNextTextElement(part))).ToUpperInvariant();
        var section = activeSection?.Trim().ToLowerInvariant();
        var workspace = roles.Contains("SUPER_ADMIN") ? (Url: "/admin/operators", Label: "Quản trị hệ thống")
            : roles.Contains("OPERATOR_OWNER") || roles.Contains("OPERATOR_DISPATCHER") ? (Url: "/operator", Label: "Điều hành nhà xe")
            : roles.Contains("STATION_AGENT") ? (Url: "/station", Label: "Vận hành bến xe")
            : (Url: "/", Label: "Tìm chuyến xe");

        return new AccountSettingsViewModel
        {
            Profile = profile ?? new CustomerProfileViewModel
            {
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber,
                Email = user.Email ?? ""
            },
            // Never return the posted password object to a Razor view.
            Password = new ChangePasswordViewModel(),
            DisplayName = displayName,
            Email = user.Email ?? "",
            Initials = string.IsNullOrEmpty(initials) ? "?" : initials,
            CreatedAtUtc = user.CreatedAtUtc,
            RoleLabels = labels.Length > 0 ? labels : ["Thành viên"],
            TwoFactorEnabled = user.TwoFactorEnabled,
            HasPassword = await userManager.HasPasswordAsync(user),
            IsCustomer = roles.Contains("CUSTOMER"),
            ActiveSection = section is "security" or "preferences" ? section : "profile",
            WorkspaceUrl = workspace.Url,
            WorkspaceLabel = workspace.Label
        };
    }

    private void ClearPasswordValues()
    {
        var fields = new[] { nameof(ChangePasswordViewModel.CurrentPassword), nameof(ChangePasswordViewModel.NewPassword), nameof(ChangePasswordViewModel.ConfirmPassword) };
        foreach (var key in ModelState.Keys.ToArray())
        {
            if (fields.Any(field => key.Equals(field, StringComparison.OrdinalIgnoreCase) || key.EndsWith("." + field, StringComparison.OrdinalIgnoreCase)))
                ModelState.SetModelValue(key, rawValue: null, attemptedValue: null);
        }
    }

    private static string ProfileErrorMessage(IdentityError error) => error.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure)
        ? "Thông tin tài khoản vừa thay đổi. Vui lòng tải lại trang và thử lại."
        : "Không thể lưu thông tin tài khoản. Vui lòng thử lại.";

    private string PasswordErrorMessage(IdentityError error) => error.Code switch
    {
        nameof(IdentityErrorDescriber.PasswordMismatch) => "Mật khẩu hiện tại chưa chính xác.",
        nameof(IdentityErrorDescriber.PasswordTooShort) => $"Mật khẩu mới cần có ít nhất {userManager.Options.Password.RequiredLength} ký tự.",
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) => "Mật khẩu mới cần có ít nhất một chữ số.",
        nameof(IdentityErrorDescriber.PasswordRequiresLower) => "Mật khẩu mới cần có ít nhất một chữ cái thường.",
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) => "Mật khẩu mới cần có ít nhất một chữ cái hoa.",
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => "Mật khẩu mới cần có ít nhất một ký tự đặc biệt.",
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => "Mật khẩu mới cần có nhiều ký tự khác nhau hơn.",
        nameof(IdentityErrorDescriber.ConcurrencyFailure) => "Thông tin tài khoản vừa thay đổi. Vui lòng tải lại trang và thử lại.",
        _ => "Không thể đổi mật khẩu. Vui lòng thử lại."
    };

    [HttpGet("tickets"), Authorize]
    public async Task<IActionResult> Tickets(CancellationToken ct)
    {
        var user = await GetActiveUserAsync();
        if (user is null)
            return Challenge();

        var bookings = await customerAccount.GetBookingsAsync(user.Id, ct);
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
        var user = await GetActiveUserAsync();
        if (user is null)
            return Challenge();

        var booking = await customerAccount.GetBookingDetailsAsync(user.Id, bookingId, ct);
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

    private async Task<ApplicationUser?> GetActiveUserAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is not null && user.IsActive)
            return user;
        await signInManager.SignOutAsync();
        return null;
    }

    private bool IsAccountReturnUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Url.IsLocalUrl(value))
            return false;
        var path = value.Split('?', '#')[0].TrimEnd('/');
        return path.Equals("/account", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/account/profile", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsOperatorReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) && value.StartsWith("/operator", StringComparison.OrdinalIgnoreCase);
    private bool IsAdminReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) &&
        (value == "/" || value.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/Home", StringComparison.OrdinalIgnoreCase));
}
