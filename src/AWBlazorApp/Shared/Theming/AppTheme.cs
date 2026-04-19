using MudBlazor;
using MudBlazor.Utilities;

namespace AWBlazorApp.Shared.Theming;

/// <summary>
/// App-wide <see cref="MudTheme"/>. Narrows the decorative palette (Primary, Info, Secondary,
/// AppBar, Surface) to blue hues + slate grays + near-black/off-white, which is what every
/// button, chip, toggle, nav item, and unspecified MudChart series inherits. Leaves the
/// semantic colors alone — <c>Success</c>, <c>Warning</c>, and <c>Error</c> still render
/// green / amber / red because their whole job is to say "good / watch this / broken" at a
/// glance. Trying to convey that through monochrome would just push the UX burden onto icons.
/// </summary>
public static class AppTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            // Decorative — blue-dominant.
            Primary           = "#1F6FEB",
            PrimaryDarken     = "#0B3D91",
            PrimaryLighten    = "#4A90E2",
            PrimaryContrastText = "#FFFFFF",
            Info              = "#4A90E2",
            InfoDarken        = "#1F6FEB",
            InfoLighten       = "#93C5FD",
            Secondary         = "#475569", // slate-600
            SecondaryDarken   = "#1F2937",
            SecondaryLighten  = "#94A3B8",
            SecondaryContrastText = "#FFFFFF",
            Tertiary          = "#0B3D91",
            TertiaryContrastText = "#FFFFFF",

            // Semantic — left alone. If you want to tune these (e.g. slightly muted reds),
            // do it here, but remember charts and KPIs lean on the visual distinctiveness.
            Success           = Colors.Green.Default,
            Warning           = Colors.Amber.Default,
            Error             = Colors.Red.Default,

            // Neutral surfaces — off-white canvas, cool-gray borders.
            Background        = "#F8FAFC",
            BackgroundGray    = "#F1F5F9",
            Surface           = "#FFFFFF",
            AppbarBackground  = "#0B3D91",
            AppbarText        = "#FFFFFF",
            DrawerBackground  = "#FFFFFF",
            DrawerText        = "#0F172A",
            DrawerIcon        = "#475569",

            TextPrimary       = "#0F172A",
            TextSecondary     = "#475569",
            TextDisabled      = new MudColor("#94A3B8").SetAlpha(0.6).ToString(MudColorOutputFormats.Hex),
            ActionDefault     = "#475569",
            ActionDisabled    = new MudColor("#94A3B8").SetAlpha(0.4).ToString(MudColorOutputFormats.Hex),
            ActionDisabledBackground = new MudColor("#CBD5E1").SetAlpha(0.2).ToString(MudColorOutputFormats.Hex),
            Divider           = "#E2E8F0",
            DividerLight      = "#F1F5F9",
            TableLines        = "#E2E8F0",
            LinesDefault      = "#E2E8F0",
            LinesInputs       = "#CBD5E1",
        },

        PaletteDark = new PaletteDark
        {
            // Brighter blue on dark surfaces so the primary still reads.
            Primary           = "#4A90E2",
            PrimaryDarken     = "#1F6FEB",
            PrimaryLighten    = "#93C5FD",
            PrimaryContrastText = "#0F172A",
            Info              = "#77B0F2",
            InfoDarken        = "#4A90E2",
            InfoLighten       = "#BFDBFE",
            Secondary         = "#94A3B8", // slate-400
            SecondaryDarken   = "#64748B",
            SecondaryLighten  = "#CBD5E1",
            SecondaryContrastText = "#0F172A",
            Tertiary          = "#93C5FD",
            TertiaryContrastText = "#0F172A",

            Success           = Colors.Green.Lighten1,
            Warning           = Colors.Amber.Lighten1,
            Error             = Colors.Red.Lighten1,

            Background        = "#0B1220",
            BackgroundGray    = "#0F172A",
            Surface           = "#0F172A",
            AppbarBackground  = "#0B3D91",
            AppbarText        = "#F1F5F9",
            DrawerBackground  = "#0F172A",
            DrawerText        = "#E2E8F0",
            DrawerIcon        = "#94A3B8",

            TextPrimary       = "#F1F5F9",
            TextSecondary     = "#94A3B8",
            TextDisabled      = new MudColor("#64748B").SetAlpha(0.6).ToString(MudColorOutputFormats.Hex),
            ActionDefault     = "#94A3B8",
            ActionDisabled    = new MudColor("#64748B").SetAlpha(0.4).ToString(MudColorOutputFormats.Hex),
            ActionDisabledBackground = new MudColor("#475569").SetAlpha(0.2).ToString(MudColorOutputFormats.Hex),
            Divider           = "#1E293B",
            DividerLight      = "#0F172A",
            TableLines        = "#1E293B",
            LinesDefault      = "#334155",
            LinesInputs       = "#475569",
        },
    };
}
