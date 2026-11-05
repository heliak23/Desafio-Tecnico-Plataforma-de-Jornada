using DndJornada.Application.Interfaces;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;
using DndJornada.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DndJornada.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher, bool includeSampleRecords = false)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Users.AnyAsync())
        {
            await SeedUsersAsync(context, hasher);
        }

        if (includeSampleRecords && !await context.TimeRecords.AnyAsync())
        {
            await SeedSampleRecordsAsync(context);
        }
    }

    public static async Task SeedUsersAsync(AppDbContext context, IPasswordHasher hasher)
    {
        var defaultPasswordHash = hasher.HashPassword("Password123!");

        // 1. RH Geral
        var adminHr = new User
        {
            Id = Guid.NewGuid(),
            Name = "Beatriz Alencar (RH)",
            Email = "admin@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.HR,
            DefaultTimezoneId = "America/Sao_Paulo",
            DailyWorkHours = 8.00m,
            Department = "Recursos Humanos"
        };

        // 2. Gestor Brasil
        var managerBr = new User
        {
            Id = Guid.NewGuid(),
            Name = "Carlos Mendes (Gestor BR)",
            Email = "gestor.brasil@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Manager,
            DefaultTimezoneId = "America/Sao_Paulo",
            DailyWorkHours = 8.00m,
            Department = "Engenharia de Software"
        };

        // 3. Gestor Europa
        var managerEu = new User
        {
            Id = Guid.NewGuid(),
            Name = "Tiago Silva (Gestor EU)",
            Email = "gestor.europa@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Manager,
            DefaultTimezoneId = "Europe/Lisbon",
            DailyWorkHours = 8.00m,
            Department = "Operações Internacionais"
        };

        // 4. Colaborador Brasil
        var employeeBr = new User
        {
            Id = Guid.NewGuid(),
            Name = "Lucas Andrade",
            Email = "colab.brasil@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Employee,
            ManagerId = managerBr.Id,
            DefaultTimezoneId = "America/Sao_Paulo",
            DailyWorkHours = 8.00m,
            Department = "Engenharia de Software"
        };

        // 5. Colaborador Viajante (caso especial do desafio)
        var employeeTraveler = new User
        {
            Id = Guid.NewGuid(),
            Name = "Mariana Costa (Viajante)",
            Email = "colab.viajante@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Employee,
            ManagerId = managerEu.Id,
            DefaultTimezoneId = "America/Sao_Paulo",
            DailyWorkHours = 8.00m,
            Department = "Operações Internacionais"
        };

        // 6. Colaborador Europa
        var employeeEu = new User
        {
            Id = Guid.NewGuid(),
            Name = "João Pereira",
            Email = "colab.europa@dndgroup.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Employee,
            ManagerId = managerEu.Id,
            DefaultTimezoneId = "Europe/Lisbon",
            DailyWorkHours = 8.00m,
            Department = "Operações Internacionais"
        };

        await context.Users.AddRangeAsync(adminHr, managerBr, managerEu, employeeBr, employeeTraveler, employeeEu);
        await context.SaveChangesAsync();
    }

    public static async Task SeedSampleRecordsAsync(AppDbContext context)
    {
        var employeeTraveler = await context.Users.FirstOrDefaultAsync(u => u.Email == "colab.viajante@dndgroup.com");
        var employeeBr = await context.Users.FirstOrDefaultAsync(u => u.Email == "colab.brasil@dndgroup.com");

        if (employeeTraveler == null) return;

        var today = DateTime.UtcNow.Date;

        var sampleRecords = new List<TimeRecord>
        {
            // Dia 1: Trabalho em São Paulo (UTC-3, offset -180)
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-2).AddHours(12),
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Type = TimeRecordType.ClockIn,
                LocationNotes = "Escritório São Paulo"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-2).AddHours(15),
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Type = TimeRecordType.LunchStart,
                LocationNotes = "Intervalo Almoço"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-2).AddHours(16),
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Type = TimeRecordType.LunchEnd,
                LocationNotes = "Retorno Almoço"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-2).AddHours(21),
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Type = TimeRecordType.ClockOut,
                LocationNotes = "Saída São Paulo"
            },

            // Dia 2: Viagem corporativa a Lisboa (UTC+0, offset 0)
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-1).AddHours(9),
                TimezoneId = "Europe/Lisbon",
                OffsetMinutes = 0,
                Type = TimeRecordType.ClockIn,
                LocationNotes = "Hub D&D Lisboa (Viagem)"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-1).AddHours(13),
                TimezoneId = "Europe/Lisbon",
                OffsetMinutes = 0,
                Type = TimeRecordType.LunchStart,
                LocationNotes = "Almoço Lisboa"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-1).AddHours(14),
                TimezoneId = "Europe/Lisbon",
                OffsetMinutes = 0,
                Type = TimeRecordType.LunchEnd,
                LocationNotes = "Retorno Lisboa"
            },
            new()
            {
                UserId = employeeTraveler.Id,
                TimestampUtc = today.AddDays(-1).AddHours(18),
                TimezoneId = "Europe/Lisbon",
                OffsetMinutes = 0,
                Type = TimeRecordType.ClockOut,
                LocationNotes = "Saída Hub Lisboa"
            }
        };

        await context.TimeRecords.AddRangeAsync(sampleRecords);

        if (employeeBr != null)
        {
            var sampleAdjustment = new TimeRecordAdjustment
            {
                UserId = employeeBr.Id,
                TargetDate = DateOnly.FromDateTime(today.AddDays(-1)),
                RequestedType = TimeRecordType.ClockOut,
                RequestedTimestampUtc = today.AddDays(-1).AddHours(21),
                TimezoneId = "America/Sao_Paulo",
                OffsetMinutes = -180,
                Reason = "Esqueci de registrar a saída devido à reunião emergencial com cliente às 18h.",
                Status = AdjustmentStatus.Pending
            };

            await context.TimeRecordAdjustments.AddAsync(sampleAdjustment);
        }

        await context.SaveChangesAsync();
    }
}
