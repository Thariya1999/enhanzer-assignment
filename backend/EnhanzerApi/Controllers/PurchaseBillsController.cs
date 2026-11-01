using System.Security.Claims;
using EnhanzerApi.Models;
using EnhanzerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnhanzerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-bills")]
public class PurchaseBillsController(IPurchaseBillService bills) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SavePurchaseBillResponse>> Create([FromBody] SavePurchaseBillRequest request, CancellationToken ct)
    {
        var user = User.FindFirstValue(ClaimTypes.Email)
                   ?? User.FindFirstValue("email")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? "unknown";
        var saved = await bills.SaveAsync(request, user, ct);
        return StatusCode(StatusCodes.Status201Created, saved);
    }
}
