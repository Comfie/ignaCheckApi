using IgnaCheck.Application.Authentication.Commands.Login;
using IgnaCheck.Application.Authentication.Commands.Register;
using IgnaCheck.Application.Authentication.Commands.RequestPasswordReset;
using IgnaCheck.Application.Authentication.Commands.ResetPassword;
using IgnaCheck.Application.Authentication.Commands.VerifyEmail;

namespace IgnaCheck.Web.Endpoints;

/// <summary>
/// Authentication endpoints for user registration, login, email verification, and password reset.
/// </summary>
public class Authentication : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .WithTags("Authentication");

        group.MapPost("register", Register)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .WithDescription("Creates a new user account with optional workspace creation. Sends email verification.")
            .Produces<Result<string>>(StatusCodes.Status200OK)
            .Produces<Result<string>>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("login", Login)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .WithDescription("Authenticates a user and returns a JWT access token.")
            .Produces<Result<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<Result<LoginResponse>>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("verify-email", VerifyEmail)
            .WithName("VerifyEmail")
            .WithSummary("Verify email address")
            .WithDescription("Verifies a user's email address using the token sent via email.")
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("request-password-reset", RequestPasswordReset)
            .WithName("RequestPasswordReset")
            .WithSummary("Request password reset")
            .WithDescription("Sends a password reset email to the user.")
            .Produces<Result>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapPost("reset-password", ResetPassword)
            .WithName("ResetPassword")
            .WithSummary("Reset password")
            .WithDescription("Resets a user's password using the token sent via email.")
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    public async Task<IResult> Register(ISender sender, RegisterCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    public async Task<IResult> Login(ISender sender, LoginCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Verify email address.
    /// </summary>
    public async Task<IResult> VerifyEmail(ISender sender, VerifyEmailCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Request password reset.
    /// </summary>
    public async Task<IResult> RequestPasswordReset(ISender sender, RequestPasswordResetCommand command)
    {
        var result = await sender.Send(command);

        // Always return 200 OK to prevent email enumeration
        return Results.Ok(result);
    }

    /// <summary>
    /// Reset password.
    /// </summary>
    public async Task<IResult> ResetPassword(ISender sender, ResetPasswordCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }
}
