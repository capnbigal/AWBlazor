namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;
public enum ExceptionType : byte
{
    ManualSequencePin = 1,
    KittingHold = 2,
    HotOrderBump = 3
}
