namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

public class CapacityExceededException : Exception
{
    public double ExcessHours { get; }
    public CapacityExceededException(double excessHours)
        : base($"Generation exceeds week capacity by {excessHours:F2} hours.")
        => ExcessHours = excessHours;
}
