using System.Security.Claims;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Back.Entities;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Tags("Auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EndpointName("Login")]
    [EndpointSummary("Login to the system")]
    [EndpointDescription("Authenticate using email and password to receive a JWT token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                title: "فشل تسجيل الدخول",
                statusCode: StatusCodes.Status401Unauthorized,
                detail: ex.Message
            );
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [EndpointName("Logout")]
    [EndpointSummary("Logout from the system")]
    [EndpointDescription("Sign out the currently authenticated user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }

    [HttpPost("refresh-token")]
    [EndpointName("RefreshToken")]
    [EndpointSummary("Refresh access token")]
    [EndpointDescription("Get a new access token using a valid refresh token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                title: "غير مصرّح",
                statusCode: StatusCodes.Status401Unauthorized,
                detail: ex.Message
            );
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    [EndpointName("RevokeToken")]
    [EndpointSummary("Revoke refresh token")]
    [EndpointDescription("Invalidate a refresh token to prevent future use")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: ex.Message
            );
        }
    }

    [HttpGet("me")]
    [Authorize]
    [EndpointName("GetCurrentUser")]
    [EndpointSummary("Get current user info")]
    [EndpointDescription("Retrieve the profile of the currently authenticated user")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                title: "غير مصرّح",
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "لم يتم التعرف على المستخدم"
            );

        try
        {
            var result = await _authService.GetCurrentUserAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: ex.Message
            );
        }
    }
}
