namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// AdventureWorks-standard single-char codes used on <see cref="Product"/>.
/// Schema constrains each field to this fixed set (see <c>Production.Product</c> check constraints).
/// </summary>
public static class ProductConstants
{
    public static readonly IReadOnlyList<ProductCodeOption> ProductLines =
    [
        new("R", "R — Road"),
        new("M", "M — Mountain"),
        new("T", "T — Touring"),
        new("S", "S — Standard"),
    ];

    public static readonly IReadOnlyList<ProductCodeOption> Classes =
    [
        new("H", "H — High"),
        new("M", "M — Medium"),
        new("L", "L — Low"),
    ];

    public static readonly IReadOnlyList<ProductCodeOption> Styles =
    [
        new("W", "W — Womens"),
        new("M", "M — Mens"),
        new("U", "U — Universal"),
    ];
}

public sealed record ProductCodeOption(string Code, string Label);
