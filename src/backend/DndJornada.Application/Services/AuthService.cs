using DndJornada.Application.DTOs;
using DndJornada.Application.Interfaces;
using DndJornada.Domain.Repositories;

namespace DndJornada.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("E-mail e senha são obrigatórios.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas ou usuário inativo.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            Token: token,
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role,
            DefaultTimezoneId: user.DefaultTimezoneId,
            Department: user.Department
        );
    }
}

