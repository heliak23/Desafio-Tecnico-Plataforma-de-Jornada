using DndJornada.Application.DTOs;

namespace DndJornada.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

