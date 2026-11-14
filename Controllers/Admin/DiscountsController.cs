using BusGoSmartTicket.Data;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusGoSmartTicket.Controllers.Admin;

[Authorize(Roles = "SUPER_ADMIN"), Route("admin/discounts")]
public sealed class DiscountsController(ApplicationDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        return View("~/Views/Membership/Manage.cshtml", new DiscountManagementViewModel { Campaigns = await db.DiscountCampaigns.OrderByDescending(x => x.StartsAtUtc).ToListAsync(ct) });
    }
    [HttpPost(""), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] DiscountForm form, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                DateTime Utc(DateTime value) => DateTime.SpecifyKind(value.AddHours(-7), DateTimeKind.Utc);
                db.DiscountCampaigns.Add(new DiscountCampaign(form.Name, form.Tier, form.Percent, Utc(form.StartsAt), Utc(form.EndsAt)));
                await db.SaveChangesAsync(ct);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex) { ModelState.AddModelError("", ex.Message); }
        }
        return View("~/Views/Membership/Manage.cshtml", new DiscountManagementViewModel { Form = form, Campaigns = await db.DiscountCampaigns.OrderByDescending(x => x.StartsAtUtc).ToListAsync(ct) });
    }
    [HttpPost("{id:guid}/deactivate"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var campaign = await db.DiscountCampaigns.FindAsync([id], ct);
        if (campaign is null)
            return NotFound();
        campaign.Deactivate();
        await db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}
