using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Web.Models;

namespace SmartTicket.Web.Controllers;

[Route("account")]
public sealed class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpGet("register")]
    public IActionResult Register() => View(new PassengerRegistrationViewModel());

    [HttpPost("register"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(PassengerRegistrationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

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
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
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
                if (IsOperatorReturnUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl!);
                return RedirectToAction("Index", "OperatorPortal");
            }
            if (await signInManager.UserManager.IsInRoleAsync(user, "STATION_AGENT"))
            {
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.StartsWith("/station", StringComparison.OrdinalIgnoreCase)) return LocalRedirect(model.ReturnUrl);
                return RedirectToAction("Index", "StationPortal");
            }
            if (IsAdminReturnUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl!);
            return RedirectToAction("Index", "Home");
        }
        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(TwoFactor), new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu chưa chính xác.");
        return View(model);
    }

    [HttpGet("two-factor")]
    public async Task<IActionResult> TwoFactor(string? returnUrl, bool rememberMe)
    {
        if (await signInManager.GetTwoFactorAuthenticationUserAsync() is null) return RedirectToAction(nameof(Login));
        return View(new TwoFactorViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    [HttpPost("two-factor"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var pendingUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, rememberClient: false);
        if (result.Succeeded)
        {
            var isOperator = pendingUser is not null && (await signInManager.UserManager.IsInRoleAsync(pendingUser, "OPERATOR_OWNER") || await signInManager.UserManager.IsInRoleAsync(pendingUser, "OPERATOR_DISPATCHER"));
            if (isOperator)
            {
                if (IsOperatorReturnUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl!);
                return RedirectToAction("Index", "OperatorPortal");
            }
            if (pendingUser is not null && await signInManager.UserManager.IsInRoleAsync(pendingUser, "STATION_AGENT"))
            {
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.StartsWith("/station", StringComparison.OrdinalIgnoreCase)) return LocalRedirect(model.ReturnUrl);
                return RedirectToAction("Index", "StationPortal");
            }
            if (IsAdminReturnUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl!);
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

    private bool IsOperatorReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) && value.StartsWith("/operator", StringComparison.OrdinalIgnoreCase);
    private bool IsAdminReturnUrl(string? value) => !string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value) &&
        (value == "/" || value.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) || value.StartsWith("/Home", StringComparison.OrdinalIgnoreCase));
}
