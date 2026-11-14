using BusGoSmartTicket.Exceptions.Operators;
using BusGoSmartTicket.Models.DTOs.Operators;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Interfaces.Operators;
using Microsoft.AspNetCore.Mvc;

namespace BusGoSmartTicket.Controllers;

[Route("operators")]
public sealed class OperatorRegistrationController(IOperatorService operators, IPrivateLicenseStore licenses) : Controller
{
    [HttpGet("register")]
    public IActionResult Register() => View("~/Views/Operators/Register.cshtml", new OperatorRegistrationForm());

    [HttpPost("register"), ValidateAntiForgeryToken, RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Register(OperatorRegistrationForm form, CancellationToken ct)
    {
        if (form.BusinessLicense is null || form.BusinessLicense.Length is 0 or > 10 * 1024 * 1024)
            ModelState.AddModelError(nameof(form.BusinessLicense), "Tải lên giấy phép dạng PDF/JPG/PNG, tối đa 10 MB.");
        var extension = Path.GetExtension(form.BusinessLicense?.FileName ?? string.Empty).ToLowerInvariant();
        if (form.BusinessLicense is not null && extension is not (".pdf" or ".jpg" or ".jpeg" or ".png"))
            ModelState.AddModelError(nameof(form.BusinessLicense), "Chỉ chấp nhận tệp PDF, JPG hoặc PNG.");
        if (!ModelState.IsValid)
            return View("~/Views/Operators/Register.cshtml", form);

        await using var stream = form.BusinessLicense!.OpenReadStream();
        if (!await HasExpectedSignatureAsync(stream, extension))
        {
            ModelState.AddModelError(nameof(form.BusinessLicense), "Nội dung tệp không khớp với định dạng khai báo.");
            return View("~/Views/Operators/Register.cshtml", form);
        }
        stream.Position = 0;
        var licensePath = await licenses.SaveAsync(stream, extension, ct);
        try
        {
            await operators.RegisterAsync(new OperatorRegistrationRequest
            {
                CompanyName = form.CompanyName,
                TaxCode = form.TaxCode,
                Hotline = form.Hotline,
                Email = form.Email,
                BusinessLicensePath = licensePath
            }, ct);
            return View("~/Views/Operators/Submitted.cshtml");
        }
        catch (OperatorConflictException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/Operators/Register.cshtml", form);
        }
    }

    private static async Task<bool> HasExpectedSignatureAsync(Stream stream, string extension)
    {
        var header = new byte[8];
        var read = await stream.ReadAsync(header);
        return extension switch
        {
            ".pdf" => read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46,
            ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            _ => false
        };
    }
}
