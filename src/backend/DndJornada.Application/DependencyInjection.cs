using DndJornada.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DndJornada.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITimeRecordCalculator, TimeRecordCalculator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITimeRecordService, TimeRecordService>();
        services.AddScoped<IAdjustmentService, AdjustmentService>();

        return services;
    }
}

