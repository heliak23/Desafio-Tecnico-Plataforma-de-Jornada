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
}

