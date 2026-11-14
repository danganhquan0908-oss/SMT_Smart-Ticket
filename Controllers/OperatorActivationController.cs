using BusGoSmartTicket.Exceptions.Operators;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Interfaces.Operators;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers;

[Route("operator-activation")]
public sealed class OperatorActivationController(IOperatorService operators) : Controller
{
    [HttpGet("start")]
    public async Task<IActionResult> Start(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return View("~/Views/Operators/ActivationExpired.cshtml");
        var activation = await operators.GetActivationAsync(token, ct);
        if (activation is null)
            return View("~/Views/Operators/ActivationExpired.cshtml");
        ViewBag.CompanyName = activation.CompanyName;
        return View("~/Views/Operators/SetPassword.cshtml", new OperatorActivationForm { Token = token, Email = activation.Email });
    }

    [HttpPost("set-password"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(OperatorActivationForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Operators/SetPassword.cshtml", form);
        try
        {
            var setup = await operators.BeginActivationAsync(form.Token, form.Password, ct);
            return View("~/Views/Operators/Authenticator.cshtml", new OperatorActivationForm { Token = form.Token, Email = setup.Email, SharedKey = setup.SharedKey });
        }
        catch (OperatorConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View("~/Views/Operators/SetPassword.cshtml", form); }
    }

    [HttpPost("verify-authenticator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyAuthenticator(OperatorActivationForm form, CancellationToken ct)
    {
        try
        {
            await operators.CompleteActivationAsync(form.Token, form.AuthenticatorCode, ct);
            TempData["Success"] = "Tài khoản đã kích hoạt. Đăng nhập để tiếp tục.";
            return RedirectToAction("Login", "Account");
        }
        catch (OperatorConflictException ex)
        {
            ModelState.AddModelError(nameof(form.AuthenticatorCode), ex.Message);
            return View("~/Views/Operators/Authenticator.cshtml", form);
        }
    }
}
