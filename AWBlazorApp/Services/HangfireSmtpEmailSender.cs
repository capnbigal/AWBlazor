using AWBlazorApp.Data;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AWBlazorApp.Services;

/// <summary>
/// Implements <see cref="IEmailSender{ApplicationUser}"/> by enqueuing a Hangfire background
/// job. The actual SMTP send happens out-of-band on a Hangfire worker, so the request that
/// triggered the email (registration, password reset, email change, etc.) returns immediately.
/// </summary>
public sealed class HangfireSmtpEmailSender(IBackgroundJobClient jobs) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => Enqueue(email, user.DisplayName ?? user.UserName, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.", isHtml: true);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => Enqueue(email, user.DisplayName ?? user.UserName, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.", isHtml: true);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => Enqueue(email, user.DisplayName ?? user.UserName, "Reset your password",
            $"Please reset your password using the following code: {resetCode}", isHtml: false);

    private Task Enqueue(string to, string? toName, string subject, string body, bool isHtml)
    {
        jobs.Enqueue<SmtpEmailJob>(j => j.SendAsync(to, toName, subject, body, isHtml));
        return Task.CompletedTask;
    }
}
