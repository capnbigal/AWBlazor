namespace AWBlazorApp.Shared.Theming;

/// <summary>
/// Small shaping helpers for categorical chart data.
/// </summary>
public static class ChartData
{
    /// <summary>
    /// Collapses the long tail of a categorical <b>count/sum</b> series into a single "Other" bucket so a
    /// skewed chart (one or two dominant categories + a long tail of tiny ones) stays readable and its
    /// axis isn't crowded with unreadable labels. Keeps the <paramref name="topN"/> largest categories by
    /// value and sums the remainder into <paramref name="otherLabel"/> (only added when there is a non-zero
    /// remainder). Returns the input unchanged when there are <paramref name="topN"/> or fewer categories.
    /// <para>
    /// Only valid for additive metrics (counts, totals, revenue, spend) — summing an averaged metric
    /// (e.g. an accuracy/variance %) into "Other" would be meaningless, so don't use it for those.
    /// </para>
    /// </summary>
    /// <param name="labels">Category labels.</param>
    /// <param name="values">Per-category values; must be the same length as <paramref name="labels"/>.</param>
    /// <param name="topN">How many of the largest categories to keep individually (must be ≥ 1).</param>
    /// <param name="otherLabel">Label for the aggregated remainder bucket.</param>
    public static (string[] Labels, double[] Values) TopN(
        string[] labels, double[] values, int topN, string otherLabel = "Other")
    {
        ArgumentNullException.ThrowIfNull(labels);
        ArgumentNullException.ThrowIfNull(values);
        if (labels.Length != values.Length)
            throw new ArgumentException("labels and values must have the same length.");
        if (topN < 1 || labels.Length <= topN)
            return (labels, values);

        var ranked = labels.Zip(values, (label, value) => (label, value))
            .OrderByDescending(x => x.value)
            .ToArray();

        var keptLabels = new List<string>(topN + 1);
        var keptValues = new List<double>(topN + 1);
        foreach (var (label, value) in ranked.Take(topN))
        {
            keptLabels.Add(label);
            keptValues.Add(value);
        }

        var otherTotal = ranked.Skip(topN).Sum(x => x.value);
        if (otherTotal > 0)
        {
            keptLabels.Add(otherLabel);
            keptValues.Add(otherTotal);
        }

        return (keptLabels.ToArray(), keptValues.ToArray());
    }
}
