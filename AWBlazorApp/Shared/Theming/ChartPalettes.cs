using MudBlazor;

namespace AWBlazorApp.Shared.Theming;

/// <summary>
/// Hex-string chart palettes, hand-tuned so every MudChart across the app inherits a consistent
/// blue/slate look. Use <see cref="BlueScale"/> for multi-series charts (line/bar time-series)
/// and <see cref="BlueOptions"/> as the shared <c>ChartOptions</c> for pie/donut charts where
/// MudBlazor would otherwise auto-cycle through its rainbow default.
/// </summary>
public static class ChartPalettes
{
    /// <summary>
    /// 10 colors, alternating between blue hues (dark → light) and slate neutrals. Tuned so
    /// adjacent slices in a pie / adjacent series in a bar chart stay visually distinct
    /// without stepping outside the brand palette. Works in both light and dark mode.
    /// </summary>
    public static readonly string[] BlueScale =
    [
        "#1F6FEB", // primary blue
        "#475569", // slate-600
        "#0B3D91", // deep navy
        "#94A3B8", // slate-400
        "#4A90E2", // mid blue
        "#1F2937", // near-black slate
        "#77B0F2", // light blue
        "#64748B", // mid slate
        "#93C5FD", // paler blue
        "#0F172A", // off-black
    ];

    /// <summary>Prebuilt <see cref="ChartOptions"/> for pie/donut charts — just drop it onto
    /// any <c>MudChart</c> via <c>ChartOptions="@ChartPalettes.BlueOptions"</c>. Safe to share
    /// the same instance across the whole app; MudBlazor only reads from it.</summary>
    public static readonly ChartOptions BlueOptions = new()
    {
        ChartPalette = BlueScale,
    };

    /// <summary>
    /// Prebuilt <see cref="BarChartOptions"/> for inline bar charts that need rotated x-axis
    /// labels so 12+ category labels (months, departments, etc.) actually fit on screen. At
    /// the default 0° rotation MudChart silently drops overlapping labels, leaving the
    /// series-name legend as the only on-screen text. 35° is the sweet spot — diagonal labels
    /// fit ~24 month names side-by-side without collision and stay readable.
    /// </summary>
    public static readonly BarChartOptions RotatedAxisOptions = new()
    {
        ChartPalette = BlueScale,
        XAxisLabelRotation = 35,
    };

    /// <summary>Same idea as <see cref="RotatedAxisOptions"/> but typed for line charts. The
    /// rotation property only exists on the chart-type-specific option classes, not the base
    /// <see cref="ChartOptions"/>, so we keep one instance per chart shape.</summary>
    public static readonly LineChartOptions RotatedAxisLineOptions = new()
    {
        ChartPalette = BlueScale,
        XAxisLabelRotation = 35,
    };
}
