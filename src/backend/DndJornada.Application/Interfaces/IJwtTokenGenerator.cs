using DndJornada.Domain.Entities;

namespace DndJornada.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

