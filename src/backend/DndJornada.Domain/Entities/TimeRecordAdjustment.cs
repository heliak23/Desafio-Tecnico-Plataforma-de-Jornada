using DndJornada.Domain.Enums;

namespace DndJornada.Domain.Entities;

public class TimeRecordAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Data da jornada que necessita do ajuste.
    /// </summary>
    public DateOnly TargetDate { get; set; }

    /// <summary>
    /// Tipo de batida que está sendo solicitada ou corrigida.
    /// </summary>
    public TimeRecordType RequestedType { get; set; }

    /// <summary>
    /// Timestamp solicitado em UTC.
    /// </summary>
    public DateTime RequestedTimestampUtc { get; set; }

    /// <summary>
    /// Fuso horário IANA informado no ajuste.
    /// </summary>
    public string TimezoneId { get; set; } = "UTC";

    /// <summary>
    /// Deslocamento em minutos em relação ao UTC.
    /// </summary>
    public int OffsetMinutes { get; set; }

    /// <summary>
    /// Motivo/justificativa apresentada pelo colaborador.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Estado atual da solicitação de ajuste (Pending, Approved, Rejected).
    /// </summary>
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

    /// <summary>
    /// Usuário (Gestor ou RH) que revisou a solicitação.
    /// </summary>
    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }

    /// <summary>
    /// Data e hora da revisão.
    /// </summary>
    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>
    /// Justificativa ou observação do revisor.
    /// </summary>
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

