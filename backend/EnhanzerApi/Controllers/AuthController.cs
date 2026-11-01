using EnhanzerApi.Models;
using EnhanzerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnhanzerApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IExternalAuthClient externalAuth,
    ILocationService locations,
    ITokenService tokens,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        ExternalLoginResult result;
        try
        {
            result = await externalAuth.LoginAsync(request.Email.Trim(), request.Password, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Could not reach the external authentication API");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Unable to reach the authentication service. Please try again later." });
        }

        if (!result.Success)
            return Unauthorized(new { message = result.Message ?? "Invalid email or password." });

        // Requirement: on successful login save the locations into Location_Details.
        await locations.SaveAsync(result.Locations, ct);

        var (token, expiresAt) = tokens.Create(request.Email.Trim());
        return Ok(new LoginResponseDto(token, expiresAt, request.Email.Trim(), result.Locations));
    }
}
