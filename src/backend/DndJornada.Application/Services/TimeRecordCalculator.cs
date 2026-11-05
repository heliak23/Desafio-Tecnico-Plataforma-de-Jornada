using System.Globalization;
using DndJornada.Application.DTOs;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;

namespace DndJornada.Application.Services;

public class TimeRecordCalculator : ITimeRecordCalculator
{
    public DailyTimeReportDto CalculateDailyReport(
        DateOnly date,
        string timezoneId,
        decimal dailyExpectedHours,
        IReadOnlyList<TimeRecord> dayRecords)
    {
        var sorted = dayRecords.OrderBy(r => r.TimestampUtc).ToList();

        var recordResponses = sorted.Select(r => new TimeRecordResponse(
            r.Id,
            r.UserId,
            r.TimestampUtc,
            r.LocalTimestamp,
            r.TimezoneId,
            r.OffsetMinutes,
            r.Type,
            r.Source,
            r.LocationNotes
        )).ToList();

        var totalWorked = TimeSpan.Zero;
        var totalBreak = TimeSpan.Zero;
        var hasIrregularity = false;
        string? irregularityReason = null;

        // Is today according to local timezone?
        var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        var expectedWork = isWeekend ? TimeSpan.Zero : TimeSpan.FromHours((double)dailyExpectedHours);

        // Algoritmo de pareamento de batidas
        DateTime? activeWorkStart = null;
        DateTime? activeBreakStart = null;

        for (int i = 0; i < sorted.Count; i++)
        {
            var current = sorted[i];

            switch (current.Type)
            {
                case TimeRecordType.ClockIn:
                    if (activeWorkStart == null)
                    {
                        activeWorkStart = current.TimestampUtc;
                    }
                    else
                    {
                        hasIrregularity = true;
                        irregularityReason = "Entrada duplicada sem saída anterior.";
                    }
                    break;

                case TimeRecordType.LunchStart:
                    if (activeWorkStart != null)
                    {
                        totalWorked += current.TimestampUtc - activeWorkStart.Value;
                        activeWorkStart = null;
                    }
                    activeBreakStart = current.TimestampUtc;
                    break;

                case TimeRecordType.LunchEnd:
                    if (activeBreakStart != null)
                    {
                        totalBreak += current.TimestampUtc - activeBreakStart.Value;
                        activeBreakStart = null;
                    }
                    activeWorkStart = current.TimestampUtc;
                    break;

                case TimeRecordType.ClockOut:
                    if (activeWorkStart != null)
                    {
                        totalWorked += current.TimestampUtc - activeWorkStart.Value;
                        activeWorkStart = null;
                    }
                    else
                    {
                        hasIrregularity = true;
                        irregularityReason = "Saída registrada sem entrada correspondente.";
                    }
                    break;
            }
        }

        // Se sobrou período aberto e a data já passou (ontem ou antes), temos uma batida esquecida/incompleta
        var todayCivil = DateOnly.FromDateTime(DateTime.UtcNow);
        if (activeWorkStart != null || activeBreakStart != null)
        {
            if (date < todayCivil)
            {
                hasIrregularity = true;
                irregularityReason = "Jornada incompleta (falta batida de saída/retorno).";
            }
        }

        var balance = totalWorked - expectedWork;
        var isPositive = balance >= TimeSpan.Zero;

        return new DailyTimeReportDto(
            Date: date,
            FormattedDate: date.ToString("dd/MM/yyyy (ddd)", new CultureInfo("pt-BR")),
            TimezoneId: timezoneId,
            Records: recordResponses,
            TotalWorkedTime: totalWorked,
            FormattedTotalWorked: FormatDuration(totalWorked),
            TotalBreakTime: totalBreak,
            FormattedTotalBreak: FormatDuration(totalBreak),
            ExpectedWorkTime: expectedWork,
            Balance: balance,
            FormattedBalance: FormatBalance(balance),
            IsPositiveBalance: isPositive,
            HasIrregularity: hasIrregularity,
            IrregularityReason: irregularityReason
        );
    }

    public MonthlyTimesheetReportDto CalculateMonthlyReport(
        User user,
        int year,
        int month,
        string timezoneId,
        IReadOnlyList<TimeRecord> monthRecords)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyReports = new List<DailyTimeReportDto>();

        var totalWorked = TimeSpan.Zero;
        var totalExpected = TimeSpan.Zero;
        var expectedElapsed = TimeSpan.Zero;

        // Determina a data civil atual no fuso informado
        var nowUtc = DateTime.UtcNow;
        DateOnly todayCivil;
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            todayCivil = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));
        }
        catch
        {
            todayCivil = DateOnly.FromDateTime(nowUtc);
        }

        bool isCurrentMonth = (year == todayCivil.Year && month == todayCivil.Month);
        bool isPastMonth = (year < todayCivil.Year || (year == todayCivil.Year && month < todayCivil.Month));

        // Agrupa os registros por LocalDate (data civil em que a batida ocorreu)
        var recordsByLocalDate = monthRecords
            .GroupBy(r => r.LocalDate)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            var recordsForDay = recordsByLocalDate.TryGetValue(date, out var list)
                ? list
                : new List<TimeRecord>();

            var daily = CalculateDailyReport(date, timezoneId, user.DailyWorkHours, recordsForDay);
            dailyReports.Add(daily);

            totalWorked += daily.TotalWorkedTime;
            totalExpected += daily.ExpectedWorkTime;

            // Saldo de banco de horas considera apenas os dias úteis já transcorridos
            if (isCurrentMonth)
            {
                if (date <= todayCivil)
                {
                    expectedElapsed += daily.ExpectedWorkTime;
                }
            }
            else if (isPastMonth)
            {
                expectedElapsed += daily.ExpectedWorkTime;
            }
        }

        // Saldo do Banco de Horas sobre os dias decorridos (não cobra dias futuros que ainda não chegaram!)
        var totalBalance = totalWorked - expectedElapsed;

        // Meta contratual padrão de 160h/mês
        var monthlyTarget = TimeSpan.FromHours(160);
        var remainingQuota = monthlyTarget > totalWorked ? monthlyTarget - totalWorked : TimeSpan.Zero;

        return new MonthlyTimesheetReportDto(
            UserId: user.Id,
            UserName: user.Name,
            Department: user.Department,
            Year: year,
            Month: month,
            Days: dailyReports,
            TotalWorked: totalWorked,
            FormattedTotalWorked: FormatDuration(totalWorked),
            TotalExpected: monthlyTarget,
            FormattedTotalExpected: "160h 00m",
            TotalBalance: totalBalance,
            FormattedTotalBalance: FormatBalance(totalBalance),
            IsPositiveTotalBalance: totalBalance >= TimeSpan.Zero,
            RemainingHours: remainingQuota,
            FormattedRemainingHours: FormatDuration(remainingQuota)
        );
    }

    public TimeRecordType DetermineNextExpectedPunch(IReadOnlyList<TimeRecord> todayRecords)
    {
        if (todayRecords == null || todayRecords.Count == 0)
        {
            return TimeRecordType.ClockIn;
        }

        var last = todayRecords.OrderBy(r => r.TimestampUtc).Last();

        return last.Type switch
        {
            TimeRecordType.ClockIn => TimeRecordType.LunchStart,
            TimeRecordType.LunchStart => TimeRecordType.LunchEnd,
            TimeRecordType.LunchEnd => TimeRecordType.ClockOut,
            TimeRecordType.ClockOut => TimeRecordType.ClockIn,
            _ => TimeRecordType.ClockIn
        };
    }

    private static string FormatDuration(TimeSpan time)
    {
        var hours = (int)Math.Abs(time.TotalHours);
        var minutes = Math.Abs(time.Minutes);
        return $"{hours:D2}h {minutes:D2}m";
    }

    private static string FormatBalance(TimeSpan time)
    {
        var sign = time < TimeSpan.Zero ? "-" : "+";
        var hours = (int)Math.Abs(time.TotalHours);
        var minutes = Math.Abs(time.Minutes);
        return $"{sign}{hours:D2}h {minutes:D2}m";
    }
}

