using DndJornada.Application.Interfaces;
using DndJornada.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public AdminController(AppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    /// <summary>
    /// Zera todas as marcações de ponto e solicitações de ajuste, mantendo as contas de usuário limpas.
    /// </summary>
    [HttpPost("reset-database")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetDatabase([FromQuery] bool includeSampleData = false)
    {
        // 1. Remove TODAS as batidas de ponto de todos os colaboradores
        _context.TimeRecords.RemoveRange(_context.TimeRecords);

        // 2. Remove TODAS as solicitações de ajuste
        _context.TimeRecordAdjustments.RemoveRange(_context.TimeRecordAdjustments);

        await _context.SaveChangesAsync();

        // 3. Garante que os usuários de teste existam
        if (!await _context.Users.AnyAsync())
        {
            await DbInitializer.SeedUsersAsync(_context, _hasher);
        }

        // 4. Se solicitado explicitamente, adiciona dados de exemplo
        if (includeSampleData)
        {
            await DbInitializer.SeedSampleRecordsAsync(_context);
        }

        return Ok(new
        {
            success = true,
            message = "Banco de dados limpo com sucesso! Todas as batidas e ocorrências foram apagadas."
        });
    }

    /// <summary>
    /// Popula novamente o banco com o histórico de exemplo para demonstração do recrutador.
    /// </summary>
    [HttpPost("seed-demo-data")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDemoData()
    {
        if (!await _context.TimeRecords.AnyAsync())
        {
            await DbInitializer.SeedSampleRecordsAsync(_context);
        }

        return Ok(new
        {
            success = true,
            message = "Dados de demonstração populados com sucesso!"
        });
    }
}
