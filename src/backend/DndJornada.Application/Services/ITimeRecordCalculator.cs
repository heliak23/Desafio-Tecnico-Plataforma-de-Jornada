using DndJornada.Application.DTOs;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;

namespace DndJornada.Application.Services;

public interface ITimeRecordCalculator
{
    DailyTimeReportDto CalculateDailyReport(
        DateOnly date,
        string timezoneId,
        decimal dailyExpectedHours,
        IReadOnlyList<TimeRecord> dayRecords);

    MonthlyTimesheetReportDto CalculateMonthlyReport(
        User user,
        int year,
        int month,
        string timezoneId,
        IReadOnlyList<TimeRecord> monthRecords);

    TimeRecordType DetermineNextExpectedPunch(IReadOnlyList<TimeRecord> todayRecords);
}

