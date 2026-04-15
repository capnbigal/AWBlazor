namespace AWBlazorApp.Services.Email;

/// <summary>
/// Bound from the "Smtp" section of appsettings.json. If <see cref="Host"/> is empty,
/// the application falls back to <see cref="Components.Account.IdentityNoOpEmailSender"/>
/// and outbound mail is logged but not delivered.
/// </summary>
public sealed class SmtpConfig
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;

    /// <summary>From-address used on outgoing mail.</summary>
    public string FromEmail { get; set; } = "noreply@example.com";
    public string? FromName { get; set; }

    /// <summary>If set, all outbound mail is redirected here instead of the real recipient.</summary>
    public string? DevToEmail { get; set; }

    /// <summary>If set, every outbound message is BCC'd to this address.</summary>
    public string? Bcc { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
