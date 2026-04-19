using System.Globalization;

namespace AWBlazorApp.Shared.Formatting;

/// <summary>Human-readable abbreviations for large numbers shown in KPI cards and chart axes.</summary>
public static class NumberFormat
{
    // AdventureWorks is a USD-denominated sample dataset. Pinning the formatter culture to
    // en-US guarantees a "$" symbol and thousands separator regardless of what the OS locale
    // is set to — without this, headless Linux containers render currencies as "¤1,234".
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Abbreviate a number using K / M / B / T suffixes. 1_234 → "1.2K", 5_898_240 → "5.9M".
    /// Values below 1_000 fall through to a plain thousands-separated integer.
    /// </summary>
    public static string Compact(double value, int decimals = 1)
    {
        var abs = Math.Abs(value);
        var format = $"0.{new string('#', decimals)}";
        if (abs >= 1_000_000_000_000) return (value / 1_000_000_000_000).ToString(format, DisplayCulture) + "T";
        if (abs >= 1_000_000_000)     return (value / 1_000_000_000    ).ToString(format, DisplayCulture) + "B";
        if (abs >= 1_000_000)         return (value / 1_000_000         ).ToString(format, DisplayCulture) + "M";
        if (abs >= 1_000)             return (value / 1_000              ).ToString(format, DisplayCulture) + "K";
        return value.ToString("N0", DisplayCulture);
    }

    public static string Compact(decimal value, int decimals = 1) => Compact((double)value, decimals);

    /// <summary>Currency-prefixed compact format, e.g. "$5.9M" or "$1,234".</summary>
    public static string CompactCurrency(double value, int decimals = 1)
    {
        if (Math.Abs(value) < 1_000)
            return value.ToString("C0", DisplayCulture);

        var symbol = DisplayCulture.NumberFormat.CurrencySymbol;
        return (value < 0 ? "-" : "") + symbol + Compact(Math.Abs(value), decimals);
    }

    public static string CompactCurrency(decimal value, int decimals = 1) => CompactCurrency((double)value, decimals);
}
