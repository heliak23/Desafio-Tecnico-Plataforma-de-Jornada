using DndJornada.Application.Services;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DndJornada.Tests;

public class TimeRecordCalculatorTests
{
    private readonly TimeRecordCalculator _calculator = new();

    [Fact]
    public void CalculateDailyReport_ShouldCalculate8HoursWorked_WhenStandardWorkday()
    {
        // Cenário: Entrada 09:00, Almoço 12:00 às 13:00, Saída 18:00 (Fuso de São Paulo UTC-3)
        var date = new DateOnly(2026, 11, 5);
        var baseUtc = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc); // 09:00 BRT
        var userId = Guid.NewGuid();

        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc, Type = TimeRecordType.ClockIn },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(3), Type = TimeRecordType.LunchStart },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(4), Type = TimeRecordType.LunchEnd },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(9), Type = TimeRecordType.ClockOut }
        };

        var report = _calculator.CalculateDailyReport(date, "America/Sao_Paulo", 8.0m, records);

        report.TotalWorkedTime.Should().Be(TimeSpan.FromHours(8));
        report.TotalBreakTime.Should().Be(TimeSpan.FromHours(1));
        report.Balance.Should().Be(TimeSpan.Zero);
        report.IsPositiveBalance.Should().BeTrue();
        report.HasIrregularity.Should().BeFalse();
    }

    [Fact]
    public void CalculateDailyReport_ShouldCalculateOvertime_WhenWorkedMoreThanExpected()
    {
        // Cenário: 9 horas trabalhadas para jornada padrão de 8 horas (+1 hora de saldo)
        var date = new DateOnly(2026, 11, 5);
        var baseUtc = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();

        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc, Type = TimeRecordType.ClockIn },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(4), Type = TimeRecordType.LunchStart },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(5), Type = TimeRecordType.LunchEnd },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(10), Type = TimeRecordType.ClockOut }
        };

        var report = _calculator.CalculateDailyReport(date, "America/Sao_Paulo", 8.0m, records);

        report.TotalWorkedTime.Should().Be(TimeSpan.FromHours(9));
        report.Balance.Should().Be(TimeSpan.FromHours(1));
        report.IsPositiveBalance.Should().BeTrue();
        report.FormattedBalance.Should().Be("+01h 00m");
    }

    [Fact]
    public void CalculateDailyReport_ShouldCalculateAccurateHours_AcrossInternationalTravelTimezones()
    {
        // Cenário: O colaborador bateu entrada em São Paulo (UTC-3) e bateu saída em Lisboa (UTC+0)
        // Como o sistema persiste em UTC absoluto, a diferença no relógio real é calculada sem erro de fuso!
        var date = new DateOnly(2026, 11, 5);
        var userId = Guid.NewGuid();

        var punchInUtc = new DateTime(2026, 11, 5, 11, 0, 0, DateTimeKind.Utc); // 08:00 em SP
        var punchOutUtc = new DateTime(2026, 11, 5, 19, 30, 0, DateTimeKind.Utc); // 19:30 em Lisboa

        var records = new List<TimeRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TimestampUtc = punchInUtc,
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Type = TimeRecordType.ClockIn
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TimestampUtc = punchOutUtc,
                TimezoneId = "Europe/Lisbon",
                OffsetMinutes = 0,
                Type = TimeRecordType.ClockOut
            }
        };

        var report = _calculator.CalculateDailyReport(date, "Europe/Lisbon", 8.0m, records);

        // 19:30 UTC - 11:00 UTC = 8h 30m trabalhadas
        report.TotalWorkedTime.Should().Be(new TimeSpan(8, 30, 0));
        report.Balance.Should().Be(new TimeSpan(0, 30, 0));
        report.FormattedBalance.Should().Be("+00h 30m");
    }

    [Theory]
    [InlineData(TimeRecordType.ClockIn, TimeRecordType.LunchStart)]
    [InlineData(TimeRecordType.LunchStart, TimeRecordType.LunchEnd)]
    [InlineData(TimeRecordType.LunchEnd, TimeRecordType.ClockOut)]
    [InlineData(TimeRecordType.ClockOut, TimeRecordType.ClockIn)]
    public void DetermineNextExpectedPunch_ShouldFollowCorrectSequence(
        TimeRecordType lastPunchType, TimeRecordType expectedNext)
    {
        var records = new List<TimeRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TimestampUtc = DateTime.UtcNow,
                Type = lastPunchType
            }
        };

        var next = _calculator.DetermineNextExpectedPunch(records);

        next.Should().Be(expectedNext);
    }

    [Fact]
    public void CalculateDailyReport_ShouldCalculate8Hours_WhenSimulatedFastForwardWorkday()
    {
        // Cenário: Simulação de avanço de tempo rápida de uma jornada inteira de 8 horas
        var date = new DateOnly(2026, 11, 5);
        var baseUtc = new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc);
        var userId = Guid.NewGuid();

        // 08:00 Entrada, +4h -> 12:00 Início Almoço, +1h -> 13:00 Retorno, +4h -> 17:00 Saída
        var records = new List<TimeRecord>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc, Type = TimeRecordType.ClockIn },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(4), Type = TimeRecordType.LunchStart },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(5), Type = TimeRecordType.LunchEnd },
            new() { Id = Guid.NewGuid(), UserId = userId, TimestampUtc = baseUtc.AddHours(9), Type = TimeRecordType.ClockOut }
        };

        var report = _calculator.CalculateDailyReport(date, "Europe/Lisbon", 8.0m, records);

        report.TotalWorkedTime.Should().Be(TimeSpan.FromHours(8));
        report.TotalBreakTime.Should().Be(TimeSpan.FromHours(1));
        report.Balance.Should().Be(TimeSpan.Zero);
        report.FormattedTotalWorked.Should().Be("08h 00m");
        report.HasIrregularity.Should().BeFalse();
    }
}

