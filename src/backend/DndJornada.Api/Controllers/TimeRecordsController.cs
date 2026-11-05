using DndJornada.Application.DTOs;
using DndJornada.Application.Services;
using DndJornada.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndJornada.Api.Controllers;

[Authorize]
[Route("api/time-records")]
[Route("api/timerecords")]
public class TimeRecordsController : BaseApiController
{
    private readonly ITimeRecordService _timeRecordService;

    public TimeRecordsController(ITimeRecordService timeRecordService)
    {
        _timeRecordService = timeRecordService;
    }

    /// <summary>
    /// Registra uma batida de ponto com auto-detecção de tipo ou tipo informado.
    /// </summary>
    [HttpPost("punch")]
    public async Task<ActionResult<TimeRecordResponse>> Punch([FromBody] ClockPunchRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _timeRecordService.PunchAsync(CurrentUserId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o espelho e cálculo da jornada de um dia específico.
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<DailyTimeReportDto>> GetDaily(
        [FromQuery] DateOnly? date,
        [FromQuery] string? timezoneId,
        CancellationToken ct)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await _timeRecordService.GetDailyReportAsync(CurrentUserId, targetDate, timezoneId, ct);
        return Ok(report);
    }

    /// <summary>
    /// Retorna o espelho de ponto completo do mês com totais e saldo de horas.
    /// </summary>
    [HttpGet("monthly")]
    public async Task<ActionResult<MonthlyTimesheetReportDto>> GetMonthly(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? timezoneId,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var report = await _timeRecordService.GetMonthlyReportAsync(CurrentUserId, targetYear, targetMonth, timezoneId, ct);
        return Ok(report);
    }

    /// <summary>
    /// Sugere o próximo tipo de batida esperado com base no histórico do dia.
    /// </summary>
    [HttpGet("next-expected")]
    public async Task<ActionResult<object>> GetNextExpectedPunch(CancellationToken ct)
    {
        var nextType = await _timeRecordService.GetNextExpectedPunchTypeAsync(CurrentUserId, ct);
        return Ok(new
        {
            type = nextType,
            description = nextType switch
            {
                TimeRecordType.ClockIn => "Entrada",
                TimeRecordType.LunchStart => "Início do Intervalo",
                TimeRecordType.LunchEnd => "Fim do Intervalo",
                TimeRecordType.ClockOut => "Saída",
                _ => "Entrada"
            }
        });
    }

    /// <summary>
    /// Retorna a última batida registrada pelo colaborador.
    /// </summary>
    [HttpGet("last")]
    public async Task<ActionResult<TimeRecordResponse>> GetLastPunch(CancellationToken ct)
    {
        var last = await _timeRecordService.GetLastPunchAsync(CurrentUserId, ct);
        if (last == null) return NoContent();
        return Ok(last);
    }
}

