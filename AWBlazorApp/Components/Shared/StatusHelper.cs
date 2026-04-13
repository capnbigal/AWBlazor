using AWBlazorApp.Data.Entities.Forecasting;
using AWBlazorApp.Data.Entities.ProcessManagement;
using MudBlazor;

namespace AWBlazorApp.Components.Shared;

/// <summary>
/// Maps status enum/byte values to human-readable labels and MudBlazor colors.
/// Used by StatusChip.razor and any component that needs status display.
/// </summary>
public static class StatusHelper
{
    // Sales Order Header status (byte 1-6)
    public static (string Label, Color Color) SalesOrderStatus(byte status) => status switch
    {
        1 => ("In Process", Color.Info),
        2 => ("Approved", Color.Primary),
        3 => ("Backordered", Color.Warning),
        4 => ("Rejected", Color.Error),
        5 => ("Shipped", Color.Success),
        6 => ("Cancelled", Color.Default),
        _ => ($"Unknown ({status})", Color.Default),
    };

    // Purchase Order Header status (byte 1-4)
    public static (string Label, Color Color) PurchaseOrderStatus(byte status) => status switch
    {
        1 => ("Pending", Color.Info),
        2 => ("Approved", Color.Primary),
        3 => ("Rejected", Color.Error),
        4 => ("Complete", Color.Success),
        _ => ($"Unknown ({status})", Color.Default),
    };

    // Forecast status
    public static (string Label, Color Color) ForForecastStatus(ForecastStatus status) => status switch
    {
        ForecastStatus.Draft => ("Draft", Color.Default),
        ForecastStatus.Active => ("Active", Color.Success),
        ForecastStatus.Archived => ("Archived", Color.Warning),
        _ => (status.ToString(), Color.Default),
    };

    // Process status
    public static (string Label, Color Color) ForProcessStatus(ProcessStatus status) => status switch
    {
        ProcessStatus.Active => ("Active", Color.Success),
        ProcessStatus.Paused => ("Paused", Color.Warning),
        ProcessStatus.Archived => ("Archived", Color.Default),
        _ => (status.ToString(), Color.Default),
    };

    // Process execution status
    public static (string Label, Color Color) ForExecutionStatus(ProcessExecutionStatus status) => status switch
    {
        ProcessExecutionStatus.Pending => ("Pending", Color.Info),
        ProcessExecutionStatus.InProgress => ("In Progress", Color.Primary),
        ProcessExecutionStatus.Completed => ("Completed", Color.Success),
        ProcessExecutionStatus.Overdue => ("Overdue", Color.Error),
        ProcessExecutionStatus.Cancelled => ("Cancelled", Color.Default),
        _ => (status.ToString(), Color.Default),
    };

    // Process step execution status
    public static (string Label, Color Color) ForStepStatus(ProcessStepExecutionStatus status) => status switch
    {
        ProcessStepExecutionStatus.Pending => ("Pending", Color.Default),
        ProcessStepExecutionStatus.InProgress => ("In Progress", Color.Info),
        ProcessStepExecutionStatus.Completed => ("Completed", Color.Success),
        ProcessStepExecutionStatus.Skipped => ("Skipped", Color.Warning),
        _ => (status.ToString(), Color.Default),
    };

    // MudTimeline icon for execution status
    public static string ExecutionStatusIcon(ProcessExecutionStatus status) => status switch
    {
        ProcessExecutionStatus.Pending => Icons.Material.Filled.Schedule,
        ProcessExecutionStatus.InProgress => Icons.Material.Filled.HourglassTop,
        ProcessExecutionStatus.Completed => Icons.Material.Filled.CheckCircle,
        ProcessExecutionStatus.Overdue => Icons.Material.Filled.Warning,
        ProcessExecutionStatus.Cancelled => Icons.Material.Filled.Cancel,
        _ => Icons.Material.Filled.HelpOutline,
    };
}
