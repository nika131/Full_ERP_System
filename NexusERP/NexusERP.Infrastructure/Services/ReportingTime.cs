using NexusERP.Domain.Exceptions;

namespace NexusERP.Infrastructure.Services;

/// <summary>
/// Interprets business calendar dates and clock hours in an IANA time zone.
/// Stored event timestamps and all query boundaries remain UTC.
/// </summary>
public static class ReportingTime
{
    public static TimeZoneInfo Resolve(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new AppException("Unsupported reporting time zone.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new AppException("Invalid reporting time zone.");
        }
    }

    public static DateTime? StartUtc(DateTime? localDate, TimeZoneInfo zone) =>
        localDate.HasValue ? ToUtcMidnight(localDate.Value.Date, zone) : null;

    public static DateTime? EndUtcExclusive(DateTime? localDate, TimeZoneInfo zone) =>
        localDate.HasValue ? ToUtcMidnight(localDate.Value.Date.AddDays(1), zone) : null;

    public static DateTime ToUtcMidnight(DateTime date, TimeZoneInfo zone)
    {
        var local = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);

        // Some zones move their clocks at midnight; midnight may not exist.
        // The day's UTC boundary is the first valid local instant.
        while (zone.IsInvalidTime(local))
            local = local.AddMinutes(1);

        // When midnight occurs twice, use the first occurrence.
        if (zone.IsAmbiguousTime(local))
            return new DateTimeOffset(local, zone.GetAmbiguousTimeOffsets(local).Max()).UtcDateTime;

        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    public static DateTime LocalTime(DateTime utc, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    // Azure SQL Server uses Windows time zone IDs, whereas browsers send IANA IDs.
    public static string SqlZone(TimeZoneInfo zone) =>
        TimeZoneInfo.TryConvertIanaIdToWindowsId(zone.Id, out var windows)
            ? windows
            : zone.Id;
}
