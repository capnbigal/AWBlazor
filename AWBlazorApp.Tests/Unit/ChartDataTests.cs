using AWBlazorApp.Shared.Theming;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Unit;

/// <summary>Pure unit tests for the <see cref="ChartData.TopN"/> categorical-chart helper (no SQL).</summary>
[TestFixture]
[Category("Unit")]
public class ChartDataTests
{
    [Test]
    public void TopN_returns_input_unchanged_when_at_or_below_n()
    {
        var labels = new[] { "A", "B", "C" };
        var values = new[] { 3.0, 2.0, 1.0 };

        var (outLabels, outValues) = ChartData.TopN(labels, values, topN: 5);

        Assert.That(outLabels, Is.EqualTo(labels));
        Assert.That(outValues, Is.EqualTo(values));
    }

    [Test]
    public void TopN_keeps_largest_and_rolls_tail_into_other()
    {
        var labels = new[] { "Production", "Sales", "Engineering", "Finance", "IT" };
        var values = new[] { 180.0, 20.0, 10.0, 5.0, 3.0 };

        var (outLabels, outValues) = ChartData.TopN(labels, values, topN: 2);

        // Top 2 by value + a summed "Other".
        Assert.That(outLabels, Is.EqualTo(new[] { "Production", "Sales", "Other" }));
        Assert.That(outValues, Is.EqualTo(new[] { 180.0, 20.0, 18.0 }));
    }

    [Test]
    public void TopN_orders_by_value_even_if_input_unsorted()
    {
        var labels = new[] { "small", "big", "mid" };
        var values = new[] { 1.0, 100.0, 50.0 };

        var (outLabels, outValues) = ChartData.TopN(labels, values, topN: 2);

        // Ranked by value desc -> big, mid kept; small (=1) is the nonzero remainder -> "Other".
        Assert.That(outLabels, Is.EqualTo(new[] { "big", "mid", "Other" }));
        Assert.That(outValues, Is.EqualTo(new[] { 100.0, 50.0, 1.0 }));
    }

    [Test]
    public void TopN_omits_other_when_remainder_is_zero()
    {
        var labels = new[] { "A", "B", "C" };
        var values = new[] { 5.0, 3.0, 0.0 };

        var (outLabels, outValues) = ChartData.TopN(labels, values, topN: 2);

        Assert.That(outLabels, Is.EqualTo(new[] { "A", "B" }));
        Assert.That(outValues, Is.EqualTo(new[] { 5.0, 3.0 }));
    }

    [Test]
    public void TopN_throws_on_length_mismatch()
    {
        Assert.Throws<ArgumentException>(() => ChartData.TopN(new[] { "A" }, new[] { 1.0, 2.0 }, topN: 1));
    }
}
