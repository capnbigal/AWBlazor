using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Qualifications.Application.Services;

/// <inheritdoc />
public sealed class QualificationService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<QualificationService> logger) : IQualificationService
{
    public async Task<int> GrantAsync(int businessEntityId, int qualificationId, DateTime earnedDate, DateTime? expiresOn,
        string? evidenceUrl, string? notes, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        // Unique constraint enforces one EmployeeQualification per (employee, qualification);
        // re-granting is treated as an update of the existing row's earned/expires/evidence.
        var existing = await db.EmployeeQualifications
            .FirstOrDefaultAsync(eq => eq.BusinessEntityId == businessEntityId && eq.QualificationId == qualificationId, ct);

        EmployeeQualification entity;
        if (existing is null)
        {
            entity = new EmployeeQualification
            {
                BusinessEntityId = businessEntityId,
                QualificationId = qualificationId,
                EarnedDate = earnedDate,
                ExpiresOn = expiresOn,
                EvidenceUrl = evidenceUrl?.Trim(),
                VerifiedByUserId = userId,
                Notes = notes?.Trim(),
                ModifiedDate = DateTime.UtcNow,
            };
            db.EmployeeQualifications.Add(entity);
            await db.SaveChangesAsync(ct);
            db.EmployeeQualificationAuditLogs.Add(EmployeeQualificationAuditService.RecordCreate(entity, userId));
        }
        else
        {
            var before = EmployeeQualificationAuditService.CaptureSnapshot(existing);
            existing.EarnedDate = earnedDate;
            existing.ExpiresOn = expiresOn;
            existing.EvidenceUrl = evidenceUrl?.Trim();
            existing.VerifiedByUserId = userId;
            existing.Notes = notes?.Trim();
            existing.ModifiedDate = DateTime.UtcNow;
            db.EmployeeQualificationAuditLogs.Add(EmployeeQualificationAuditService.RecordUpdate(before, existing, userId));
            entity = existing;
        }
        await db.SaveChangesAsync(ct);

        // Auto-resolve any open QualificationAlerts that this grant would clear.
        var openAlerts = await db.QualificationAlerts
            .Where(a => a.BusinessEntityId == businessEntityId && a.QualificationId == qualificationId
                        && a.Status == QualificationAlertStatus.Open)
            .ToListAsync(ct);
        foreach (var alert in openAlerts)
        {
            alert.Status = QualificationAlertStatus.Resolved;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.AcknowledgedByUserId = userId;
            alert.Notes = (alert.Notes is null ? "" : alert.Notes + " ") + "(auto-resolved by qualification grant)";
            alert.ModifiedDate = DateTime.UtcNow;
        }
        if (openAlerts.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Granted qual {Q} to employee {E} — auto-resolved {N} alert(s)",
                qualificationId, businessEntityId, openAlerts.Count);
        }

        return entity.Id;
    }

    public async Task RevokeAsync(int employeeQualificationId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var entity = await db.EmployeeQualifications.FirstOrDefaultAsync(eq => eq.Id == employeeQualificationId, ct)
            ?? throw new InvalidOperationException($"EmployeeQualification {employeeQualificationId} not found.");
        db.EmployeeQualifications.Remove(entity);
        db.EmployeeQualificationAuditLogs.Add(EmployeeQualificationAuditService.RecordDelete(entity, userId));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Revoked qual {Q} from employee {E}", entity.QualificationId, entity.BusinessEntityId);
    }
}
