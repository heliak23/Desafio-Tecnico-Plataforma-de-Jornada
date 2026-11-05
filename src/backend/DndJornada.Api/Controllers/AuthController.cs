using DndJornada.Application.DTOs;
using DndJornada.Application.Services;
using DndJornada.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndJornada.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser(CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(CurrentUserId, ct);
        if (user == null) return NotFound(new { message = "Usuário não encontrado." });

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.DefaultTimezoneId,
            user.DailyWorkHours,
            user.Department
        });
    }

    [HttpGet("demo-accounts")]
    [AllowAnonymous]
    public ActionResult GetDemoAccounts()
    {
        var accounts = new[]
        {
            new { Role = "RH Geral", Email = "admin@dndgroup.com", Password = "Password123!", Description = "Acesso total, relatórios e fechamentos" },
            new { Role = "Gestor (Brasil)", Email = "gestor.brasil@dndgroup.com", Password = "Password123!", Description = "Gestão da equipe BR e aprovações" },
            new { Role = "Gestor (Europa)", Email = "gestor.europa@dndgroup.com", Password = "Password123!", Description = "Gestão da equipe EU e aprovações" },
            new { Role = "Colaborador (Brasil)", Email = "colab.brasil@dndgroup.com", Password = "Password123!", Description = "Horário de Brasília (UTC-3)" },
            new { Role = "Colaborador (Viajante)", Email = "colab.viajante@dndgroup.com", Password = "Password123!", Description = "Viagens frequentes entre BR e Portugal (UTC-3 / UTC+0)" },
            new { Role = "Colaborador (Europa)", Email = "colab.europa@dndgroup.com", Password = "Password123!", Description = "Horário de Lisboa (UTC+0 / UTC+1)" }
        };

        return Ok(accounts);
    }
}

