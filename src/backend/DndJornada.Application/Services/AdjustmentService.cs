using DndJornada.Application.DTOs;
using DndJornada.Domain.Entities;
using DndJornada.Domain.Enums;
using DndJornada.Domain.Repositories;

namespace DndJornada.Application.Services;

public class AdjustmentService : IAdjustmentService
{
    private readonly ITimeRecordAdjustmentRepository _adjustmentRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IUserRepository _userRepository;

    public AdjustmentService(
        ITimeRecordAdjustmentRepository adjustmentRepository,
        ITimeRecordRepository timeRecordRepository,
        IUserRepository userRepository)
    {
        _adjustmentRepository = adjustmentRepository;
        _timeRecordRepository = timeRecordRepository;
        _userRepository = userRepository;
    }

    public async Task<AdjustmentResponse> CreateRequestAsync(Guid userId, CreateAdjustmentRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Colaborador não encontrado.");

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ArgumentException("A justificativa é obrigatória para solicitar ajuste de ponto.");
        }

        // Calcula UTC a partir da hora local e offset informados
        var localDto = new DateTimeOffset(request.RequestedLocalTime, TimeSpan.FromMinutes(request.OffsetMinutes));
        var timestampUtc = localDto.UtcDateTime;

        var adjustment = new TimeRecordAdjustment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetDate = request.TargetDate,
            RequestedType = request.RequestedType,
            RequestedTimestampUtc = timestampUtc,
            TimezoneId = request.TimezoneId,
            OffsetMinutes = request.OffsetMinutes,
            Reason = request.Reason.Trim(),
            Status = AdjustmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _adjustmentRepository.AddAsync(adjustment, ct);

        return MapToResponse(adjustment, user.Name);
    }

    public async Task<IReadOnlyList<AdjustmentResponse>> GetMyAdjustmentsAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _adjustmentRepository.GetByUserIdAsync(userId, ct);
        return list.Select(a => MapToResponse(a, a.User?.Name ?? string.Empty)).ToList();
    }

    public async Task<IReadOnlyList<AdjustmentResponse>> GetPendingForManagerAsync(Guid managerId, CancellationToken ct = default)
    {
        var list = await _adjustmentRepository.GetPendingByManagerIdAsync(managerId, ct);
        return list.Select(a => MapToResponse(a, a.User?.Name ?? string.Empty)).ToList();
    }

    public async Task<IReadOnlyList<AdjustmentResponse>> GetAllPendingAsync(CancellationToken ct = default)
    {
        var list = await _adjustmentRepository.GetAllPendingAsync(ct);
        return list.Select(a => MapToResponse(a, a.User?.Name ?? string.Empty)).ToList();
    }

    public async Task<AdjustmentResponse> ReviewAsync(Guid adjustmentId, Guid reviewerId, ReviewAdjustmentRequest request, CancellationToken ct = default)
    {
        var adjustment = await _adjustmentRepository.GetByIdAsync(adjustmentId, ct)
            ?? throw new KeyNotFoundException("Solicitação de ajuste não encontrada.");

        if (adjustment.Status != AdjustmentStatus.Pending)
        {
            throw new InvalidOperationException("Esta solicitação já foi avaliada anteriormente.");
        }

        var reviewer = await _userRepository.GetByIdAsync(reviewerId, ct)
            ?? throw new KeyNotFoundException("Avaliador não encontrado.");

        adjustment.Status = request.Status;
        adjustment.ReviewedById = reviewerId;
        adjustment.ReviewedAtUtc = DateTime.UtcNow;
        adjustment.ReviewNotes = request.ReviewNotes;

        // Se foi aprovado, insere automaticamente o registro de ponto correspondente!
        if (request.Status == AdjustmentStatus.Approved)
        {
            var record = new TimeRecord
            {
                Id = Guid.NewGuid(),
                UserId = adjustment.UserId,
                TimestampUtc = adjustment.RequestedTimestampUtc,
                TimezoneId = adjustment.TimezoneId,
                OffsetMinutes = adjustment.OffsetMinutes,
                Type = adjustment.RequestedType,
                Source = RecordSource.ManualAdjustment,
                LocationNotes = $"Ajuste aprovado por {reviewer.Name}: {adjustment.Reason}",
                CreatedAtUtc = DateTime.UtcNow
            };

            await _timeRecordRepository.AddAsync(record, ct);
        }

        await _adjustmentRepository.UpdateAsync(adjustment, ct);

        return MapToResponse(adjustment, adjustment.User?.Name ?? string.Empty, reviewer.Name);
    }

    private static AdjustmentResponse MapToResponse(TimeRecordAdjustment a, string userName, string? reviewerName = null)
    {
        var requestedLocal = new DateTimeOffset(
            DateTime.SpecifyKind(a.RequestedTimestampUtc, DateTimeKind.Utc))
            .ToOffset(TimeSpan.FromMinutes(a.OffsetMinutes));

        return new AdjustmentResponse(
            Id: a.Id,
            UserId: a.UserId,
            UserName: userName,
            TargetDate: a.TargetDate,
            RequestedType: a.RequestedType,
            RequestedTimestampUtc: a.RequestedTimestampUtc,
            RequestedLocalTime: requestedLocal,
            TimezoneId: a.TimezoneId,
            OffsetMinutes: a.OffsetMinutes,
            Reason: a.Reason,
            Status: a.Status,
            ReviewedById: a.ReviewedById,
            ReviewedByName: reviewerName ?? a.ReviewedBy?.Name,
            ReviewedAtUtc: a.ReviewedAtUtc,
            ReviewNotes: a.ReviewNotes,
            CreatedAtUtc: a.CreatedAtUtc
        );
    }
}

