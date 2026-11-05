using DndJornada.Application.Interfaces;
using DndJornada.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Zera o banco de dados e recria os dados iniciais de teste instantaneamente.
    /// </summary>
    [HttpPost("reset-database")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetDatabase()
    {
        await _context.Database.EnsureDeletedAsync();
        await DbInitializer.SeedAsync(_context, _hasher);

        return Ok(new
        {
            success = true,
            message = "Banco de dados zerado e dados iniciais de teste recriados com sucesso!"
        });
    }
}

