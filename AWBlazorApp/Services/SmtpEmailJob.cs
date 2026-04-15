using System.Net.Mail;
using Hangfire;
using Microsoft.Extensions.Options;

namespace AWBlazorApp.Services;

/// <summary>
/// Hangfire background job that delivers a single transactional email via System.Net.Mail.
/// </summary>
public sealed class SmtpEmailJob(IOptions<SmtpConfig> config, ILogger<SmtpEmailJob> logger)
{
    private readonly SmtpConfig _config = config.Value;

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 600 })]
    public Task SendAsync(string to, string? toName, string subject, string body, bool isHtml) =>
        SendWithAttachmentAsync(to, toName, subject, body, isHtml, null, null);

    /// <summary>
    /// Variant that accepts a single in-memory attachment. attachmentBytes may be null to send
    /// without — used by scheduled email reports to ship the rendered CSV alongside the body.
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 600 })]
    public async Task SendWithAttachmentAsync(
        string to, string? toName, string subject, string body, bool isHtml,
        byte[]? attachmentBytes, string? attachmentName)
    {
        if (!_config.IsConfigured)
        {
            logger.LogWarning("SMTP not configured — dropping email to {To} ({Subject})", to, subject);
            return;
        }

        using var client = new SmtpClient(_config.Host!, _config.Port)
        {
            EnableSsl = _config.EnableSsl,
            Credentials = string.IsNullOrEmpty(_config.Username)
                ? null
                : new System.Net.NetworkCredential(_config.Username, _config.Password),
        };

        var emailTo = _config.DevToEmail is not null
            ? new MailAddress(_config.DevToEmail)
            : new MailAddress(to, toName);

        var emailFrom = new MailAddress(_config.FromEmail, _config.FromName);

        using var msg = new MailMessage(emailFrom, emailTo)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml,
        };

        if (_config.Bcc is not null)
        {
            msg.Bcc.Add(new MailAddress(_config.Bcc));
        }

        Attachment? attachment = null;
        if (attachmentBytes is { Length: > 0 } && !string.IsNullOrEmpty(attachmentName))
        {
            attachment = new Attachment(new MemoryStream(attachmentBytes), attachmentName);
            msg.Attachments.Add(attachment);
        }

        try
        {
            logger.LogInformation("Sending email to {To} (subject: {Subject}, attachment: {Attachment})",
                emailTo.Address, subject, attachmentName ?? "none");
            await client.SendMailAsync(msg);
        }
        finally
        {
            attachment?.Dispose();
        }
    }
}
