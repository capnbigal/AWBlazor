using AWBlazorApp.Features.Scheduling.Services;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Features.Scheduling.Services;

[TestFixture]
public class IsoWeekHelperTests
{
    [Test]
    public void FromDate_RegularWeek_Encodes_YearTimes100PlusWeek()
    {
        // Wed 2026-04-29 is in 2026-W18
        var id = IsoWeekHelper.FromDate(new DateTime(2026, 4, 29));
        Assert.That(id, Is.EqualTo(202618));
    }

    [Test]
    public void FromDate_YearBoundary_UsesIsoYear_NotCalendarYear()
    {
        // 2027-01-01 is a Friday; ISO week belongs to 2026-W53
        var id = IsoWeekHelper.FromDate(new DateTime(2027, 1, 1));
        Assert.That(id, Is.EqualTo(202653));
    }

    [Test]
    public void ToMondayUtc_ReturnsMidnightMondayOfIsoWeek()
    {
        var monday = IsoWeekHelper.ToMondayUtc(202618);
        Assert.That(monday, Is.EqualTo(new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Test]
    public void FromDate_MondayAndSunday_SameIsoWeek()
    {
        var mondayId = IsoWeekHelper.FromDate(new DateTime(2026, 4, 27));
        var sundayId = IsoWeekHelper.FromDate(new DateTime(2026, 5, 3));
        Assert.That(mondayId, Is.EqualTo(sundayId));
        Assert.That(mondayId, Is.EqualTo(202618));
    }
}
