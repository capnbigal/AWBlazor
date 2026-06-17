namespace AWBlazorApp.Features.Scheduling.Rules.Domain;
public enum RecalcActionType : byte
{
    SoftResort = 1,
    AlertOnly = 2,
    HardReplan = 3
}
