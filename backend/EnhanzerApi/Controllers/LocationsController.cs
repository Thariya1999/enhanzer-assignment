using EnhanzerApi.Models;
using EnhanzerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnhanzerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public class LocationsController(ILocationService locations) : ControllerBase
{
    /// <summary>Rows from the Location_Details table (used to fill the Batch dropdown).</summary>
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> Get(CancellationToken ct) =>
        Ok(await locations.GetAllAsync(ct));
}
