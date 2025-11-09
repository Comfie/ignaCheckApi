using IgnaCheck.Application.Authentication.Commands.Login;
using IgnaCheck.Application.Authentication.Commands.Register;
using IgnaCheck.Application.Authentication.Commands.RequestPasswordReset;
using IgnaCheck.Application.Authentication.Commands.ResetPassword;
using IgnaCheck.Application.Authentication.Commands.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Authentication controller for user registration, login, email verification, and password reset.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly ISender _sender;

    public AuthenticationController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>User ID if successful</returns>
    /// <response code="200">User registered successfully. Check email for verification link.</response>
    /// <response code="400">Invalid registration data or user already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>JWT access token and user details</returns>
    /// <response code="200">Login successful. Returns JWT access token.</response>
    /// <response code="400">Invalid credentials or email not verified.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verify email address.
    /// </summary>
    /// <param name="command">Email verification token and user ID</param>
    /// <returns>Success if email verified</returns>
    /// <response code="200">Email verified successfully. Welcome email sent.</response>
    /// <response code="400">Invalid or expired verification token.</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Request password reset.
    /// </summary>
    /// <param name="command">Email address</param>
    /// <returns>Always returns success to prevent email enumeration</returns>
    /// <response code="200">If email exists, password reset link has been sent.</response>
    [HttpPost("request-password-reset")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result>> RequestPasswordReset([FromBody] RequestPasswordResetCommand command)
    {
        var result = await _sender.Send(command);

        // Always return 200 OK to prevent email enumeration
        return Ok(result);
    }

    /// <summary>
    /// Reset password with token.
    /// </summary>
    /// <param name="command">Reset token, user ID, and new password</param>
    /// <returns>Success if password reset</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid or expired reset token.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
