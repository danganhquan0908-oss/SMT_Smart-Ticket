using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.Application.Operations;

namespace SmartTicket.Web.Controllers;

[Authorize(Roles = "OPERATOR_OWNER"), Route("operator/finance")]
public sealed class OperatorFinanceController(IStationOperationsService operations) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View("~/Views/Operators/Finance.cshtml", await operations.GetSettlementsAsync(UserId(), ct));
    [HttpPost("payout-account"), ValidateAntiForgeryToken]
    public async Task<IActionResult> PayoutAccount(PayoutAccountRequest request, CancellationToken ct)
    { if (!ModelState.IsValid) { TempData["Error"] = "Thông tin tài khoản nhận tiền chưa hợp lệ."; return RedirectToAction(nameof(Index)); } try { await operations.SetPayoutAccountAsync(UserId(), request, ct); TempData["Success"] = "Đã cập nhật tài khoản nhận tiền."; } catch (OperationsConflictException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    [HttpPost("trips/{tripId:guid}/complete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid tripId, CancellationToken ct)
    { try { await operations.CompleteTripAsync(UserId(), tripId, ct); TempData["Success"] = "Đã hoàn tất chuyến. Settlement sẽ được tạo tự động."; } catch (OperationsConflictException ex) { TempData["Error"] = ex.Message; } catch (OperationsNotFoundException) { return NotFound(); } return RedirectToAction("Trips", "OperatorPortal"); }
    [HttpPost("settlements/{settlementId:guid}/payout-trigger"), ValidateAntiForgeryToken]
    public async Task<IActionResult> TriggerPayout(Guid settlementId, CancellationToken ct)
    {
        try
        {
            await operations.TriggerSettlementPayoutAsync(UserId(), settlementId, ct);
            TempData["Success"] = "Đã thực hiện lệnh giải ngân Napas 247 thành công.";
        }
        catch (OperationsConflictException ex) { TempData["Error"] = ex.Message; }
        catch (OperationsNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
