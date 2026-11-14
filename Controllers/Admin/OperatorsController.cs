using BusGoSmartTicket.Exceptions.Operators;
using BusGoSmartTicket.Services.Interfaces.Operators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers.Admin;

[Authorize(Roles = "SUPER_ADMIN")]
[Route("admin/operators")]
public sealed class OperatorsController(IOperatorService operators, IPrivateLicenseStore licenses, IActivationEmailSender emailSender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View("~/Views/Admin/Operators/Index.cshtml", await operators.GetApplicationsAsync(ct));

    [HttpPost("{id:guid}/approve"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var notice = await operators.ReviewAsync(id, true, null, ct);
        if (notice is null)
        {
            TempData["Error"] = "Không tìm thấy hồ sơ đang chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }
        var link = Url.Action("Start", "OperatorActivation", new
        {
            token = notice.Token
        }, Request.Scheme)!;
        await emailSender.SendAsync(notice.Email, link, ct);
        TempData["Success"] = $"Đã duyệt hồ sơ và gửi hướng dẫn kích hoạt đến {notice.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/reject"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string note, CancellationToken ct)
    {
        await operators.ReviewAsync(id, false, note, ct);
        TempData["Success"] = "Đã từ chối hồ sơ nhà xe.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/resend-activation"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendActivation(Guid id, CancellationToken ct)
    {
        var notice = await operators.ResendActivationAsync(id, ct);
        var link = Url.Action("Start", "OperatorActivation", new
        {
            token = notice.Token
        }, Request.Scheme)!;
        await emailSender.SendAsync(notice.Email, link, ct);
        TempData["Success"] = $"Đã gửi lại liên kết kích hoạt đến {notice.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/license")]
    public async Task<IActionResult> License(Guid id, CancellationToken ct)
    {
        try
        {
            var path = await operators.GetLicensePathAsync(id, ct);
            var file = await licenses.OpenAsync(path, ct);
            return file is null ? NotFound() : File(file.Value.Content, file.Value.ContentType, file.Value.DownloadName);
        }
        catch (OperatorNotFoundException) { return NotFound(); }
    }
}
