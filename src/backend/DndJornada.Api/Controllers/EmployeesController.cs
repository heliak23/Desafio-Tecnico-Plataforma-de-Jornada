using DndJornada.Application.DTOs;
using DndJornada.Application.Services;
using DndJornada.Domain.Enums;
using DndJornada.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndJornada.Api.Controllers;

[Authorize(Roles = "Manager,HR")]
public class EmployeesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ITimeRecordService _timeRecordService;

    public EmployeesController(IUserRepository userRepository, ITimeRecordService timeRecordService)
    {
        _userRepository = userRepository;
        _timeRecordService = timeRecordService;
    }

    [HttpGet]
    public async Task<ActionResult> GetEmployees(CancellationToken ct)
    {
        IReadOnlyList<Domain.Entities.User> employees;

        if (CurrentUserRole == UserRole.HR)
        {
            employees = await _userRepository.GetAllAsync(ct);
        }
        else
        {
            employees = await _userRepository.GetSubordinatesAsync(CurrentUserId, ct);
        }

        var result = employees.Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.Role,
            u.DefaultTimezoneId,
            u.DailyWorkHours,
            u.Department,
            ManagerName = u.Manager?.Name
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}/monthly")]
    public async Task<ActionResult<MonthlyTimesheetReportDto>> GetEmployeeMonthly(
        [FromRoute] Guid id,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? timezoneId,
        CancellationToken ct)
    {
        var targetUser = await _userRepository.GetByIdAsync(id, ct);
        if (targetUser == null) return NotFound(new { message = "Colaborador não encontrado." });

        // Se for gestor, só pode ver membros da sua equipe
        if (CurrentUserRole == UserRole.Manager && targetUser.ManagerId != CurrentUserId)
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var report = await _timeRecordService.GetMonthlyReportAsync(id, targetYear, targetMonth, timezoneId, ct);
        return Ok(report);
    }

    /// <summary>
    /// Retorna o resumo consolidado de fechamento mensal de todos os funcionários (Acesso restrito ao RH).
    /// </summary>
    [Authorize(Roles = "HR")]
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<TeamMonthlySummaryReportDto>> GetTeamMonthlySummary(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        var employees = await _userRepository.GetAllAsync(ct);

        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var items = new List<TeamMonthlySummaryItemDto>();
        TimeSpan totalWorkedCompany = TimeSpan.Zero;
        TimeSpan totalBalanceCompany = TimeSpan.Zero;
        int totalIrregularities = 0;

        foreach (var emp in employees)
        {
            var report = await _timeRecordService.GetMonthlyReportAsync(emp.Id, targetYear, targetMonth, emp.DefaultTimezoneId, ct);
            
            var irregularities = report.Days.Count(d => d.HasIrregularity);
            var activeDays = report.Days.Count(d => d.Records.Any());

            totalWorkedCompany += report.TotalWorked;
            totalBalanceCompany += report.TotalBalance;
            totalIrregularities += irregularities;

            items.Add(new TeamMonthlySummaryItemDto(
                emp.Id,
                emp.Name,
                emp.Email,
                emp.Department,
                emp.DefaultTimezoneId,
                emp.DailyWorkHours,
                report.FormattedTotalWorked,
                report.FormattedTotalExpected,
                report.FormattedTotalBalance,
                report.IsPositiveTotalBalance,
                irregularities,
                activeDays,
                report.FormattedRemainingHours
            ));
        }

        var isPositiveBalance = totalBalanceCompany >= TimeSpan.Zero;
        var sign = isPositiveBalance ? "+" : "-";
        var absBalance = totalBalanceCompany.Duration();
        var formattedBalanceCompany = $"{sign}{(int)absBalance.TotalHours:D2}h {absBalance.Minutes:D2}m";
        var formattedWorkedCompany = $"{(int)totalWorkedCompany.TotalHours:D2}h {totalWorkedCompany.Minutes:D2}m";

        var summary = new TeamMonthlySummaryReportDto(
            targetYear,
            targetMonth,
            employees.Count,
            formattedWorkedCompany,
            formattedBalanceCompany,
            isPositiveBalance,
            totalIrregularities,
            items
        );

        return Ok(summary);
    }
}

