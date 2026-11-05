using DndJornada.Domain.Enums;

namespace DndJornada.Domain.Entities;

public class TimeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Momento universal absoluto em que a batida foi registrada (100% UTC).
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Identificador IANA do fuso de onde a batida foi feita (ex: "America/Sao_Paulo", "Europe/Lisbon", "Europe/Madrid").
    /// </summary>
    public string TimezoneId { get; set; } = "UTC";

    /// <summary>
    /// Deslocamento em minutos em relação ao UTC no momento exato (ex: -180 para UTC-3, +60 para UTC+1).
    /// </summary>
    public int OffsetMinutes { get; set; }

    /// <summary>
    /// Tipo de batida (Entrada, Início Almoço, Fim Almoço, Saída).
    /// </summary>
    public TimeRecordType Type { get; set; }

    /// <summary>
    /// Origem do registro (Web, Mobile, Ajuste Manual).
    /// </summary>
    public RecordSource Source { get; set; } = RecordSource.Web;

    /// <summary>
    /// Notas ou localização informada (ex: "Trabalho Remoto - São Paulo", "Viagem corporativa - Lisboa").
    /// </summary>
    public string? LocationNotes { get; set; }

    /// <summary>
    /// Data de criação do registro no sistema.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data e hora local no fuso em que a batida foi efetuada.
    /// </summary>
    public DateTimeOffset LocalTimestamp =>
        new DateTimeOffset(DateTime.SpecifyKind(TimestampUtc, DateTimeKind.Utc))
            .ToOffset(TimeSpan.FromMinutes(OffsetMinutes));

    /// <summary>
    /// Data civil local da batida (usada para agrupar as batidas do mesmo dia de trabalho).
    /// </summary>
    public DateOnly LocalDate => DateOnly.FromDateTime(LocalTimestamp.DateTime);
}

