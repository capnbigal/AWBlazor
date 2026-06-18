using AWBlazorApp.Shared.Services;
using MudBlazor;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Unit;

/// <summary>
/// Pure unit tests for the <see cref="RecordActivity"/> timeline synthesizer (no SQL). Part of the
/// fast <c>[Category("Unit")]</c> tier.
/// </summary>
[TestFixture]
[NUnit.Framework.Category("Unit")]
public class RecordActivityTests
{
    [Test]
    public void WorkOrder_in_progress_has_no_completed_event()
    {
        var start = new DateTime(2026, 1, 1);
        var events = RecordActivity.ForWorkOrder(start, start.AddDays(10), endDate: null);

        Assert.That(events, Has.Some.Matches<ActivityEvent>(e => e.Label == "In progress"));
        Assert.That(events, Has.None.Matches<ActivityEvent>(e => e.Label == "Completed"));
    }

    [Test]
    public void WorkOrder_completed_shows_success_completed_event()
    {
        var start = new DateTime(2026, 1, 1);
        var events = RecordActivity.ForWorkOrder(start, start.AddDays(10), endDate: start.AddDays(8));

        Assert.That(events, Has.Some.Matches<ActivityEvent>(e => e.Label == "Completed" && e.Color == Color.Success));
    }

    [Test]
    public void Dated_events_are_in_chronological_order()
    {
        var start = new DateTime(2026, 1, 1);
        var events = RecordActivity.ForSalesOrder(start, start.AddDays(5), shipDate: start.AddDays(3), status: 5);

        var dated = events.Where(e => e.At.HasValue).Select(e => e.At!.Value).ToList();
        Assert.That(dated, Is.Ordered);
    }

    [Test]
    public void SalesOrder_not_shipped_shows_pending_shipment()
    {
        var start = new DateTime(2026, 1, 1);
        var events = RecordActivity.ForSalesOrder(start, start.AddDays(5), shipDate: null, status: 1);

        Assert.That(events, Has.Some.Matches<ActivityEvent>(e => e.Label == "Not yet shipped"));
    }

    [Test]
    public void PurchaseOrder_includes_current_status_milestone()
    {
        var start = new DateTime(2026, 1, 1);
        var events = RecordActivity.ForPurchaseOrder(start, shipDate: start.AddDays(2), status: 4);

        // status 4 = "Complete" per StatusHelper.PurchaseOrderStatus
        Assert.That(events, Has.Some.Matches<ActivityEvent>(e => e.Label == "Complete"));
    }
}
