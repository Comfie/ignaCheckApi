namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification message to a user.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name for personalization</param>
    /// <param name="verificationToken">Email verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailVerificationAsync(
        string email,
        string? firstName,
        string verificationToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to a user.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name for personalization</param>
    /// <param name="resetToken">Password reset token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPasswordResetAsync(
        string email,
        string? firstName,
        string resetToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a workspace invitation email.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name for personalization</param>
    /// <param name="organizationName">Name of the organization/workspace</param>
    /// <param name="inviterName">Name of the person who sent the invitation</param>
    /// <param name="role">Role the user will have in the workspace</param>
    /// <param name="invitationToken">Invitation token</param>
    /// <param name="message">Optional personal message from the inviter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendWorkspaceInvitationAsync(
        string email,
        string? firstName,
        string organizationName,
        string inviterName,
        string role,
        string invitationToken,
        string? message = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to a newly registered user.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendWelcomeEmailAsync(
        string email,
        string? firstName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a generic email.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="textBody">Plain text body content (fallback)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);
}
