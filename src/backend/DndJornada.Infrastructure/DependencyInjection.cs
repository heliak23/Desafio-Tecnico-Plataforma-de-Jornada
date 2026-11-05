using DndJornada.Application.Interfaces;
using DndJornada.Domain.Repositories;
using DndJornada.Infrastructure.Data;
using DndJornada.Infrastructure.Repositories;
using DndJornada.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DndJornada.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=dnd_jornada_db;Username=dnd_user;Password=dnd_secret_password";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITimeRecordRepository, TimeRecordRepository>();
        services.AddScoped<ITimeRecordAdjustmentRepository, TimeRecordAdjustmentRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

