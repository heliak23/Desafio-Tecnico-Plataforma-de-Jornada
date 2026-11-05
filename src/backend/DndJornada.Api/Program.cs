using System.Text;
using DndJornada.Application;
using DndJornada.Application.Interfaces;
using DndJornada.Infrastructure;
using DndJornada.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection das Camadas (Clean Architecture)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Autenticação JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "DungeonsAndDragonsGroupSuperSecretKeyForJwtAuthentication2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DndJornadaApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DndJornadaClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. Configuração de CORS (para o Frontend Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Swagger com suporte a Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "D&D Group — API de Gestão de Jornada de Trabalho",
        Version = "v1",
        Description = "API RESTful para controle de jornada internacional com suporte a multi-fuso horário (Brasil e Europa)."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 5. Inicialização e Seed do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DbInitializer.SeedAsync(context, hasher);
        app.Logger.LogInformation("Banco de dados verificado e populado com sucesso.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
    }
}

// 6. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "D&D Group Jornada API v1");
    });
}

app.UseCors("AllowAngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
