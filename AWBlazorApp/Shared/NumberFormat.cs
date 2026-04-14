using System.Globalization;

namespace AWBlazorApp.Shared;

/// <summary>Human-readable abbreviations for large numbers shown in KPI cards and chart axes.</summary>
public static class NumberFormat
{
    /// <summary>
    /// Abbreviate a number using K / M / B / T suffixes. 1_234 → "1.2K", 5_898_240 → "5.9M".
    /// Values below 1_000 fall through to a plain thousands-separated integer.
    /// </summary>
    public static string Compact(double value, int decimals = 1)
    {
        var abs = Math.Abs(value);
        if (abs >= 1_000_000_000_000) return (value / 1_000_000_000_000).ToString($"0.{new string('#', decimals)}", CultureInfo.InvariantCulture) + "T";
        if (abs >= 1_000_000_000)     return (value / 1_000_000_000    ).ToString($"0.{new string('#', decimals)}", CultureInfo.InvariantCulture) + "B";
        if (abs >= 1_000_000)         return (value / 1_000_000         ).ToString($"0.{new string('#', decimals)}", CultureInfo.InvariantCulture) + "M";
        if (abs >= 1_000)             return (value / 1_000              ).ToString($"0.{new string('#', decimals)}", CultureInfo.InvariantCulture) + "K";
        return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    public static string Compact(decimal value, int decimals = 1) => Compact((double)value, decimals);

    /// <summary>Currency-prefixed compact format, e.g. "$5.9M" or "$1,234".</summary>
    public static string CompactCurrency(double value, int decimals = 1)
    {
        var symbol = NumberFormatInfo.CurrentInfo.CurrencySymbol;
        return Math.Abs(value) >= 1_000
            ? (value < 0 ? "-" : "") + symbol + Compact(Math.Abs(value), decimals)
            : value.ToString("C0", CultureInfo.CurrentCulture);
    }

    public static string CompactCurrency(decimal value, int decimals = 1) => CompactCurrency((double)value, decimals);
}
