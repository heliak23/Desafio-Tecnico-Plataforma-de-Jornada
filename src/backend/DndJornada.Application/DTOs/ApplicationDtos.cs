using DndJornada.Domain.Enums;

namespace DndJornada.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    Guid UserId,
    string Name,
    string Email,
    UserRole Role,
    string DefaultTimezoneId,
    string Department
);

public record ClockPunchRequest(
    TimeRecordType? Type,
    string? TimezoneId,
    int? OffsetMinutes,
    string? LocationNotes,
    DateTime? SimulatedTimestampUtc = null
);

public record TimeRecordResponse(
    Guid Id,
    Guid UserId,
    DateTime TimestampUtc,
    DateTimeOffset LocalTimestamp,
    string TimezoneId,
    int OffsetMinutes,
    TimeRecordType Type,
    RecordSource Source,
    string? LocationNotes
);

public record DailyTimeReportDto(
    DateOnly Date,
    string FormattedDate,
    string TimezoneId,
    List<TimeRecordResponse> Records,
    TimeSpan TotalWorkedTime,
    string FormattedTotalWorked,
    TimeSpan TotalBreakTime,
    string FormattedTotalBreak,
    TimeSpan ExpectedWorkTime,
    TimeSpan Balance,
    string FormattedBalance,
    bool IsPositiveBalance,
    bool HasIrregularity,
    string? IrregularityReason
);

public record MonthlyTimesheetReportDto(
    Guid UserId,
    string UserName,
    string Department,
    int Year,
    int Month,
    List<DailyTimeReportDto> Days,
    TimeSpan TotalWorked,
    string FormattedTotalWorked,
    TimeSpan TotalExpected,
    string FormattedTotalExpected,
    TimeSpan TotalBalance,
    string FormattedTotalBalance,
    bool IsPositiveTotalBalance
);

public record CreateAdjustmentRequest(
    DateOnly TargetDate,
    TimeRecordType RequestedType,
    DateTime RequestedLocalTime,
    string TimezoneId,
    int OffsetMinutes,
    string Reason
);

public record ReviewAdjustmentRequest(
    AdjustmentStatus Status,
    string? ReviewNotes
);

public record AdjustmentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    DateOnly TargetDate,
    TimeRecordType RequestedType,
    DateTime RequestedTimestampUtc,
    DateTimeOffset RequestedLocalTime,
    string TimezoneId,
    int OffsetMinutes,
    string Reason,
    AdjustmentStatus Status,
    Guid? ReviewedById,
    string? ReviewedByName,
    DateTime? ReviewedAtUtc,
    string? ReviewNotes,
    DateTime CreatedAtUtc
);

