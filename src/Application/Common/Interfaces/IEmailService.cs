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

    /// <summary>
    /// Sends a finding assignment notification email.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="findingTitle">Title of the assigned finding</param>
    /// <param name="projectName">Name of the project</param>
    /// <param name="severity">Severity level of the finding</param>
    /// <param name="dueDate">Due date for the finding (optional)</param>
    /// <param name="findingLink">Link to the finding</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendFindingAssignedAsync(
        string email,
        string? firstName,
        string findingTitle,
        string projectName,
        string severity,
        DateTime? dueDate,
        string findingLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a mention notification email (finding or task comment).
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="mentionedBy">Name of user who mentioned</param>
    /// <param name="entityType">Type of entity (Finding or Task)</param>
    /// <param name="entityTitle">Title of the finding or task</param>
    /// <param name="commentPreview">Preview of the comment</param>
    /// <param name="entityLink">Link to the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendMentionNotificationAsync(
        string email,
        string? firstName,
        string mentionedBy,
        string entityType,
        string entityTitle,
        string commentPreview,
        string entityLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an audit check completion notification.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="projectName">Name of the project</param>
    /// <param name="frameworkName">Name of the framework</param>
    /// <param name="complianceScore">Overall compliance score</param>
    /// <param name="totalFindings">Total number of findings</param>
    /// <param name="criticalFindings">Number of critical findings</param>
    /// <param name="dashboardLink">Link to the compliance dashboard</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAuditCheckCompletedAsync(
        string email,
        string? firstName,
        string projectName,
        string frameworkName,
        decimal complianceScore,
        int totalFindings,
        int criticalFindings,
        string dashboardLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a task assignment notification email.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="taskTitle">Title of the assigned task</param>
    /// <param name="projectName">Name of the project</param>
    /// <param name="priority">Priority level of the task</param>
    /// <param name="dueDate">Due date for the task (optional)</param>
    /// <param name="taskLink">Link to the task</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendTaskAssignedAsync(
        string email,
        string? firstName,
        string taskTitle,
        string projectName,
        string priority,
        DateTime? dueDate,
        string taskLink,
        CancellationToken cancellationToken = default);
}
