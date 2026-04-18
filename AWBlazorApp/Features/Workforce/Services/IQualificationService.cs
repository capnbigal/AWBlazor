using AWBlazorApp.Features.Workforce.Domain;

namespace AWBlazorApp.Features.Workforce.Services;

/// <summary>
/// Owns the qualification grant + revocation paths and the expiry computation. Use this
/// service rather than touching <see cref="EmployeeQualification"/> directly so the
/// audit log + alert resolution logic stays consistent.
/// </summary>
public interface IQualificationService
{
    Task<int> GrantAsync(int businessEntityId, int qualificationId, DateTime earnedDate, DateTime? expiresOn,
        string? evidenceUrl, string? notes, string? userId, CancellationToken cancellationToken);
    Task RevokeAsync(int employeeQualificationId, string? userId, CancellationToken cancellationToken);

    /// <summary>Computed status for an EmployeeQualification: Current / SoonExpiring (within
    /// <paramref name="warningWindowDays"/>) / Expired / NoExpiry.</summary>
    static QualificationStatus EvaluateStatus(EmployeeQualification q, DateTime now, int warningWindowDays = 30)
    {
        if (q.ExpiresOn is null) return QualificationStatus.NoExpiry;
        var days = (q.ExpiresOn.Value - now).TotalDays;
        if (days < 0) return QualificationStatus.Expired;
        if (days <= warningWindowDays) return QualificationStatus.SoonExpiring;
        return QualificationStatus.Current;
    }
}

public enum QualificationStatus
{
    Current,
    NoExpiry,
    SoonExpiring,
    Expired,
}
