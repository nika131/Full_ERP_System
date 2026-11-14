using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Services;

namespace NexusERP.Tests;

public class ReportingTimeTests
{
    [Fact]
    public void GeorgianSale_StoresSameInstantInUtc()
    {
        var zone = ReportingTime.Resolve("Asia/Tbilisi");
        var utc = ReportingTime.ToUtcMidnight(new DateTime(2026, 11, 15), zone);
        Assert.Equal(new DateTime(2026, 11, 14, 20, 0, 0, DateTimeKind.Utc), utc);

        var sale = new DateTime(2026, 11, 14, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(14, ReportingTime.LocalTime(sale, zone).Hour);
    }

    [Fact]
    public void NewYorkBusinessDay_UsesDaylightSavingOffsetAtEachBoundary()
    {
        var zone = ReportingTime.Resolve("America/New_York");
        var summerStart = ReportingTime.ToUtcMidnight(new DateTime(2026, 7, 1), zone);
        var winterStart = ReportingTime.ToUtcMidnight(new DateTime(2026, 12, 1), zone);

        Assert.Equal(new DateTime(2026, 7, 1, 4, 0, 0, DateTimeKind.Utc), summerStart);
        Assert.Equal(new DateTime(2026, 12, 1, 5, 0, 0, DateTimeKind.Utc), winterStart);
    }

    [Fact]
    public void NewYorkSpringForwardDay_HasTwentyThreeUtcHours()
    {
        var zone = ReportingTime.Resolve("America/New_York");
        var start = ReportingTime.ToUtcMidnight(new DateTime(2026, 3, 8), zone);
        var end = ReportingTime.ToUtcMidnight(new DateTime(2026, 3, 9), zone);

        Assert.Equal(TimeSpan.FromHours(23), end - start);
    }

    [Fact]
    public void NewYorkFallBackDay_HasTwentyFiveUtcHours()
    {
        var zone = ReportingTime.Resolve("America/New_York");
        var start = ReportingTime.ToUtcMidnight(new DateTime(2026, 11, 1), zone);
        var end = ReportingTime.ToUtcMidnight(new DateTime(2026, 11, 2), zone);

        Assert.Equal(TimeSpan.FromHours(25), end - start);
    }

    [Fact]
    public void InvalidTimeZone_IsRejected()
    {
        Assert.Throws<AppException>(() => ReportingTime.Resolve("Not/A_Real_Zone"));
    }
}
