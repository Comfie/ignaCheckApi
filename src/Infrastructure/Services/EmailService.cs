using IgnaCheck.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Infrastructure.Services;

/// <summary>
/// Email service implementation.
/// Currently uses console logging for development.
/// TODO: Integrate with real email provider (SendGrid, AWS SES, SMTP, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _baseUrl = configuration["App:BaseUrl"] ?? "https://app.ignacheck.ai";
    }

    public async Task SendEmailVerificationAsync(
        string email,
        string? firstName,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_baseUrl}/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";

        var subject = "Verify your IgnaCheck.ai email address";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
        <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 14px;"">The Copilot for Compliance</p>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            Welcome to IgnaCheck.ai! Please verify your email address to complete your registration.
        </p>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{verificationUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                Verify Email Address
            </a>
        </div>

        <p style=""color: #6b7280; font-size: 14px; margin-top: 30px;"">
            If the button doesn't work, copy and paste this link into your browser:
        </p>
        <p style=""color: #6b7280; font-size: 12px; word-break: break-all; background: #f3f4f6; padding: 10px; border-radius: 4px;"">
            {verificationUrl}
        </p>

        <p style=""color: #9ca3af; font-size: 13px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;"">
            This link will expire in 24 hours. If you didn't create an account, you can safely ignore this email.
        </p>
    </div>

    <div style=""text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px;"">
        <p>© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.</p>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

Welcome to IgnaCheck.ai! Please verify your email address to complete your registration.

Click the link below to verify your email:
{verificationUrl}

This link will expire in 24 hours. If you didn't create an account, you can safely ignore this email.

---
© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string email,
        string? firstName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_baseUrl}/auth/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";

        var subject = "Reset your IgnaCheck.ai password";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
        <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 14px;"">The Copilot for Compliance</p>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            We received a request to reset your password. Click the button below to create a new password.
        </p>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                Reset Password
            </a>
        </div>

        <p style=""color: #6b7280; font-size: 14px; margin-top: 30px;"">
            If the button doesn't work, copy and paste this link into your browser:
        </p>
        <p style=""color: #6b7280; font-size: 12px; word-break: break-all; background: #f3f4f6; padding: 10px; border-radius: 4px;"">
            {resetUrl}
        </p>

        <p style=""color: #9ca3af; font-size: 13px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;"">
            This link will expire in 1 hour. If you didn't request a password reset, you can safely ignore this email.
        </p>
    </div>

    <div style=""text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px;"">
        <p>© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.</p>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

We received a request to reset your password. Click the link below to create a new password:
{resetUrl}

This link will expire in 1 hour. If you didn't request a password reset, you can safely ignore this email.

---
© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendWorkspaceInvitationAsync(
        string email,
        string? firstName,
        string organizationName,
        string inviterName,
        string role,
        string invitationToken,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        var acceptUrl = $"{_baseUrl}/auth/accept-invitation?token={Uri.EscapeDataString(invitationToken)}";
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";

        var subject = $"{inviterName} invited you to join {organizationName} on IgnaCheck.ai";
        var personalMessage = !string.IsNullOrEmpty(message)
            ? $@"<div style=""background: #f9fafb; padding: 15px; border-left: 4px solid #667eea; margin: 20px 0;"">
                    <p style=""color: #4b5563; font-style: italic; margin: 0;"">""{message}""</p>
                    <p style=""color: #9ca3af; font-size: 13px; margin: 10px 0 0 0;"">— {inviterName}</p>
                 </div>"
            : "";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
        <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 14px;"">The Copilot for Compliance</p>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            <strong>{inviterName}</strong> has invited you to join <strong>{organizationName}</strong> on IgnaCheck.ai as a <strong>{role}</strong>.
        </p>

        {personalMessage}

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{acceptUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                Accept Invitation
            </a>
        </div>

        <p style=""color: #6b7280; font-size: 14px; margin-top: 30px;"">
            If the button doesn't work, copy and paste this link into your browser:
        </p>
        <p style=""color: #6b7280; font-size: 12px; word-break: break-all; background: #f3f4f6; padding: 10px; border-radius: 4px;"">
            {acceptUrl}
        </p>

        <p style=""color: #9ca3af; font-size: 13px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;"">
            This invitation will expire in 7 days. If you don't want to join, you can safely ignore this email.
        </p>
    </div>

    <div style=""text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px;"">
        <p>© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.</p>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

{inviterName} has invited you to join {organizationName} on IgnaCheck.ai as a {role}.

{(!string.IsNullOrEmpty(message) ? $@"Message from {inviterName}:
""{message}""

" : "")}Click the link below to accept the invitation:
{acceptUrl}

This invitation will expire in 7 days. If you don't want to join, you can safely ignore this email.

---
© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string email,
        string? firstName,
        CancellationToken cancellationToken = default)
    {
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";
        var dashboardUrl = $"{_baseUrl}/dashboard";

        var subject = "Welcome to IgnaCheck.ai!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">Welcome to IgnaCheck.ai! 🎉</h1>
        <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 14px;"">The Copilot for Compliance</p>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            Your account is all set up! You're now ready to revolutionize how you handle compliance audits.
        </p>

        <h3 style=""color: #1f2937; margin-top: 30px;"">Get Started:</h3>
        <ul style=""color: #4b5563; font-size: 15px; line-height: 1.8;"">
            <li>Create your first workspace</li>
            <li>Set up a compliance project</li>
            <li>Upload your documentation</li>
            <li>Let AI analyze your compliance gaps</li>
        </ul>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{dashboardUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                Go to Dashboard
            </a>
        </div>

        <p style=""color: #6b7280; font-size: 14px; margin-top: 30px;"">
            Need help getting started? Check out our <a href=""{_baseUrl}/docs"" style=""color: #667eea; text-decoration: none;"">documentation</a> or reach out to our support team.
        </p>

        <p style=""color: #9ca3af; font-size: 13px; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb;"">
            Remember: We have to work twice as hard to get half of what they have. Let's build something exceptional. 💪
        </p>
    </div>

    <div style=""text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px;"">
        <p>© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.</p>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

Welcome to IgnaCheck.ai! Your account is all set up and you're ready to revolutionize how you handle compliance audits.

Get Started:
- Create your first workspace
- Set up a compliance project
- Upload your documentation
- Let AI analyze your compliance gaps

Visit your dashboard: {dashboardUrl}

Need help? Check out our documentation at {_baseUrl}/docs or reach out to our support team.

Remember: We have to work twice as hard to get half of what they have. Let's build something exceptional.

---
© {DateTime.UtcNow.Year} IgnaCheck.ai. All rights reserved.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual email provider integration (SendGrid, AWS SES, SMTP, etc.)
        // For now, log to console for development

        _logger.LogInformation(
            "Sending email to {To} with subject: {Subject}",
            to,
            subject);

        _logger.LogDebug(
            "Email content:\n{HtmlBody}",
            htmlBody);

        // Simulate async email sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Email sent successfully to {To}", to);
    }

    public async Task SendFindingAssignedAsync(
        string email,
        string? firstName,
        string findingTitle,
        string projectName,
        string severity,
        DateTime? dueDate,
        string findingLink,
        CancellationToken cancellationToken = default)
    {
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";
        var dueDateText = dueDate.HasValue ? $"Due: {dueDate.Value:MMM dd, yyyy}" : "No due date set";

        var subject = $"Finding assigned: {findingTitle}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            A new finding has been assigned to you in <strong>{projectName}</strong>.
        </p>

        <div style=""background: #f9fafb; padding: 20px; border-radius: 6px; margin: 20px 0;"">
            <h3 style=""color: #1f2937; margin-top: 0;"">{findingTitle}</h3>
            <p style=""color: #6b7280; margin: 10px 0;""><strong>Severity:</strong> {severity}</p>
            <p style=""color: #6b7280; margin: 10px 0;"">{dueDateText}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{findingLink}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                View Finding
            </a>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

A new finding has been assigned to you in {projectName}.

Finding: {findingTitle}
Severity: {severity}
{dueDateText}

View finding: {findingLink}";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendMentionNotificationAsync(
        string email,
        string? firstName,
        string mentionedBy,
        string entityType,
        string entityTitle,
        string commentPreview,
        string entityLink,
        CancellationToken cancellationToken = default)
    {
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";

        var subject = $"{mentionedBy} mentioned you in a comment";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            <strong>{mentionedBy}</strong> mentioned you in a comment on {entityType}: <strong>{entityTitle}</strong>.
        </p>

        <div style=""background: #f9fafb; padding: 15px; border-left: 4px solid #667eea; margin: 20px 0;"">
            <p style=""color: #4b5563; font-style: italic; margin: 0;"">{commentPreview}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{entityLink}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                View Comment
            </a>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

{mentionedBy} mentioned you in a comment on {entityType}: {entityTitle}.

""{commentPreview}""

View comment: {entityLink}";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendAuditCheckCompletedAsync(
        string email,
        string? firstName,
        string projectName,
        string frameworkName,
        decimal complianceScore,
        int totalFindings,
        int criticalFindings,
        string dashboardLink,
        CancellationToken cancellationToken = default)
    {
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";

        var subject = $"Audit check completed: {projectName}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            The audit check for <strong>{projectName}</strong> against <strong>{frameworkName}</strong> has been completed.
        </p>

        <div style=""background: #f9fafb; padding: 20px; border-radius: 6px; margin: 20px 0;"">
            <h3 style=""color: #1f2937; margin-top: 0;"">Results Summary</h3>
            <p style=""color: #6b7280; margin: 10px 0;""><strong>Compliance Score:</strong> {complianceScore:F1}%</p>
            <p style=""color: #6b7280; margin: 10px 0;""><strong>Total Findings:</strong> {totalFindings}</p>
            <p style=""color: #6b7280; margin: 10px 0;""><strong>Critical Findings:</strong> {criticalFindings}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{dashboardLink}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                View Dashboard
            </a>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

The audit check for {projectName} against {frameworkName} has been completed.

Results Summary:
- Compliance Score: {complianceScore:F1}%
- Total Findings: {totalFindings}
- Critical Findings: {criticalFindings}

View dashboard: {dashboardLink}";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendTaskAssignedAsync(
        string email,
        string? firstName,
        string taskTitle,
        string projectName,
        string priority,
        DateTime? dueDate,
        string taskLink,
        CancellationToken cancellationToken = default)
    {
        var name = !string.IsNullOrEmpty(firstName) ? firstName : "there";
        var dueDateText = dueDate.HasValue ? $"Due: {dueDate.Value:MMM dd, yyyy}" : "No due date set";

        var subject = $"Task assigned: {taskTitle}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 28px;"">IgnaCheck.ai</h1>
    </div>

    <div style=""background: #ffffff; padding: 40px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1f2937; margin-top: 0;"">Hi {name},</h2>

        <p style=""color: #4b5563; font-size: 16px;"">
            A new task has been assigned to you in <strong>{projectName}</strong>.
        </p>

        <div style=""background: #f9fafb; padding: 20px; border-radius: 6px; margin: 20px 0;"">
            <h3 style=""color: #1f2937; margin-top: 0;"">{taskTitle}</h3>
            <p style=""color: #6b7280; margin: 10px 0;""><strong>Priority:</strong> {priority}</p>
            <p style=""color: #6b7280; margin: 10px 0;"">{dueDateText}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{taskLink}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;"">
                View Task
            </a>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Hi {name},

A new task has been assigned to you in {projectName}.

Task: {taskTitle}
Priority: {priority}
{dueDateText}

View task: {taskLink}";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }
}
