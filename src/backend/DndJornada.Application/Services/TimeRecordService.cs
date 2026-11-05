using DndJornada.Application.DTOs;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;
using DndJornada.Domain.Repositories;

namespace DndJornada.Application.Services;

public class TimeRecordService : ITimeRecordService
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITimeRecordCalculator _calculator;

    public TimeRecordService(
        ITimeRecordRepository timeRecordRepository,
        IUserRepository userRepository,
        ITimeRecordCalculator calculator)
    {
        _timeRecordRepository = timeRecordRepository;
        _userRepository = userRepository;
        _calculator = calculator;
    }

    public async Task<TimeRecordResponse> PunchAsync(Guid userId, ClockPunchRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Colaborador não encontrado.");

        var timezoneId = !string.IsNullOrWhiteSpace(request.TimezoneId)
            ? request.TimezoneId
            : user.DefaultTimezoneId;

        // Resolve offset
        var nowUtc = DateTime.UtcNow;
        int offsetMinutes;

        if (request.OffsetMinutes.HasValue)
        {
            offsetMinutes = request.OffsetMinutes.Value;
        }
        else
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                offsetMinutes = (int)tz.GetUtcOffset(nowUtc).TotalMinutes;
            }
            catch
            {
                offsetMinutes = 0;
            }
        }

        // Validação anti-duplicidade (evita duplo clique acidental em menos de 30 segundos)
        var lastPunch = await _timeRecordRepository.GetLastRecordByUserAsync(userId, ct);
        if (lastPunch != null && (nowUtc - lastPunch.TimestampUtc).TotalSeconds < 30)
        {
            throw new InvalidOperationException("Batida duplicada detectada. Por favor, aguarde 30 segundos entre batidas.");
        }

        // Determina o tipo de batida
        TimeRecordType punchType;
        if (request.Type.HasValue)
        {
            punchType = request.Type.Value;
        }
        else
        {
            // Busca as batidas de hoje para sugerir a próxima
            var todayCivil = DateOnly.FromDateTime(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc).AddMinutes(offsetMinutes));
            var todayRecords = await _timeRecordRepository.GetByUserAndDateAsync(userId, todayCivil, timezoneId, ct);
            punchType = _calculator.DetermineNextExpectedPunch(todayRecords);
        }

        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TimestampUtc = nowUtc,
            TimezoneId = timezoneId,
            OffsetMinutes = offsetMinutes,
            Type = punchType,
            Source = RecordSource.Web,
            LocationNotes = request.LocationNotes,
            CreatedAtUtc = nowUtc
        };

        await _timeRecordRepository.AddAsync(record, ct);

        return new TimeRecordResponse(
            record.Id,
            record.UserId,
            record.TimestampUtc,
            record.LocalTimestamp,
            record.TimezoneId,
            record.OffsetMinutes,
            record.Type,
            record.Source,
            record.LocationNotes
        );
    }

    public async Task<DailyTimeReportDto> GetDailyReportAsync(Guid userId, DateOnly date, string? timezoneId = null, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Colaborador não encontrado.");

        var tz = !string.IsNullOrWhiteSpace(timezoneId) ? timezoneId : user.DefaultTimezoneId;
        var records = await _timeRecordRepository.GetByUserAndDateAsync(userId, date, tz, ct);

        return _calculator.CalculateDailyReport(date, tz, user.DailyWorkHours, records);
    }

    public async Task<MonthlyTimesheetReportDto> GetMonthlyReportAsync(Guid userId, int year, int month, string? timezoneId = null, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Colaborador não encontrado.");

        var tz = !string.IsNullOrWhiteSpace(timezoneId) ? timezoneId : user.DefaultTimezoneId;

        var startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);
        var endOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc).AddDays(1);

        var records = await _timeRecordRepository.GetByUserAndDateRangeUtcAsync(userId, startOfMonth, endOfMonth, ct);

        return _calculator.CalculateMonthlyReport(user, year, month, tz, records);
    }

    public async Task<TimeRecordType> GetNextExpectedPunchTypeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Colaborador não encontrado.");

        var todayRecords = await _timeRecordRepository.GetByUserAndDateAsync(
            userId, DateOnly.FromDateTime(DateTime.UtcNow), user.DefaultTimezoneId, ct);

        return _calculator.DetermineNextExpectedPunch(todayRecords);
    }

    public async Task<TimeRecordResponse?> GetLastPunchAsync(Guid userId, CancellationToken ct = default)
    {
        var last = await _timeRecordRepository.GetLastRecordByUserAsync(userId, ct);
        if (last == null) return null;

        return new TimeRecordResponse(
            last.Id,
            last.UserId,
            last.TimestampUtc,
            last.LocalTimestamp,
            last.TimezoneId,
            last.OffsetMinutes,
            last.Type,
            last.Source,
            last.LocationNotes
        );
    }
}

