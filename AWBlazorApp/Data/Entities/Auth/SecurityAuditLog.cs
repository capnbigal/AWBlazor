namespace AWBlazorApp.Data.Entities.Auth;

public class SecurityAuditLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty; // Login, PasswordChange, ApiKeyGenerated, RoleGranted
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}
