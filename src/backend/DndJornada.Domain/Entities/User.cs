using DndJornada.Domain.Enums;

namespace DndJornada.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    
    // Fuso padrão de contratação (ex: "America/Sao_Paulo" ou "Europe/Lisbon")
    public string DefaultTimezoneId { get; set; } = "America/Sao_Paulo";
    
    // Carga horária padrão diária em horas (ex: 8.0)
    public decimal DailyWorkHours { get; set; } = 8.0m;
    
    // Departamento / Área (ex: "Tecnologia", "Vendas", "Operações")
    public string Department { get; set; } = string.Empty;
    
    // Gestor direto (para fluxo de aprovação de pontos)
    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }
    
    public ICollection<User> Subordinates { get; set; } = new List<User>();
    public ICollection<TimeRecord> TimeRecords { get; set; } = new List<TimeRecord>();
    public ICollection<TimeRecordAdjustment> Adjustments { get; set; } = new List<TimeRecordAdjustment>();
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

