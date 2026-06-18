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

    /// <summary>
    /// Distinct-hue palette for CATEGORICAL charts (pie / donut), where each slice is an unrelated
    /// category — territories, channels, departments — and the eye needs to tell adjacent slices
    /// apart. The all-blue <see cref="BlueScale"/> made a 10-territory donut read as one blob;
    /// these hues stay tasteful (brand blue leads) while being individually legible in light + dark.
    /// </summary>
    public static readonly string[] Categorical =
    [
        "#1F6FEB", // brand blue
        "#14B8A6", // teal
        "#F59E0B", // amber
        "#8B5CF6", // violet
        "#10B981", // emerald
        "#EF4444", // red
        "#0EA5E9", // sky
        "#EC4899", // pink
        "#F97316", // orange
        "#64748B", // slate
    ];

    /// <summary>Prebuilt <see cref="ChartOptions"/> for pie/donut charts — drop onto any
    /// <c>MudChart</c> via <c>ChartOptions="@ChartPalettes.CategoricalOptions"</c>. Uses the
    /// distinct-hue <see cref="Categorical"/> palette so slices are distinguishable.</summary>
    public static readonly ChartOptions CategoricalOptions = new()
    {
        ChartPalette = Categorical,
    };

    /// <summary>Back-compat alias for the pie/donut options. Existing charts reference
    /// <c>BlueOptions</c>; it now points at <see cref="CategoricalOptions"/> so multi-slice charts
    /// are legible (the old monochrome blue scale made adjacent slices indistinguishable). New
    /// charts should use <see cref="CategoricalOptions"/>.</summary>
    public static readonly ChartOptions BlueOptions = CategoricalOptions;

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
