using DndJornada.Application.DTOs;
using DndJornada.Domain.Enums;

namespace DndJornada.Application.Services;

public interface ITimeRecordService
{
    Task<TimeRecordResponse> PunchAsync(Guid userId, ClockPunchRequest request, CancellationToken ct = default);
    Task<DailyTimeReportDto> GetDailyReportAsync(Guid userId, DateOnly date, string? timezoneId = null, CancellationToken ct = default);
    Task<MonthlyTimesheetReportDto> GetMonthlyReportAsync(Guid userId, int year, int month, string? timezoneId = null, CancellationToken ct = default);
    Task<TimeRecordType> GetNextExpectedPunchTypeAsync(Guid userId, DateTime? referenceUtc = null, string? timezoneId = null, CancellationToken ct = default);
    Task<TimeRecordResponse?> GetLastPunchAsync(Guid userId, CancellationToken ct = default);
}

