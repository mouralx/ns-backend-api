using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IAvailabilityRepository _availabilityRepository;

    public AvailabilityService(IAvailabilityRepository availabilityRepository)
    {
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<IEnumerable<AvailabilityRuleDto>>> GetRulesAsync(Guid therapistId)
    {
        var rules = await _availabilityRepository.GetAllRulesAsync();
        var filteredRules = rules.Where(r => r.TherapistId == therapistId);

        var dtos = filteredRules.Select(r => new AvailabilityRuleDto(
            r.Id,
            r.DayOfWeek,
            r.StartTime,
            r.EndTime,
            r.IsActive)).ToList();

        return Result<IEnumerable<AvailabilityRuleDto>>.Success(dtos);
    }

    public async Task<Result<AvailabilityRuleDto>> CreateRuleAsync(Guid therapistId, CreateRuleRequest request)
    {
        var rule = new AvailabilityRule
        {
            TherapistId = therapistId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true
        };

        var created = await _availabilityRepository.AddRuleAsync(rule);

        var dto = new AvailabilityRuleDto(
            created.Id,
            created.DayOfWeek,
            created.StartTime,
            created.EndTime,
            created.IsActive);

        return Result<AvailabilityRuleDto>.Success(dto);
    }

    public async Task<Result> DeleteRuleAsync(Guid ruleId)
    {
        await _availabilityRepository.RemoveRuleAsync(ruleId);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<AvailabilityBlockDto>>> GetBlocksAsync(Guid therapistId)
    {
        var blocks = await _availabilityRepository.GetBlocksAsync(therapistId);

        var dtos = blocks.Select(b => new AvailabilityBlockDto(
            b.Id,
            b.StartAt,
            b.EndAt,
            b.Reason,
            b.IsRecurrence,
            b.RecurrenceRule)).ToList();

        return Result<IEnumerable<AvailabilityBlockDto>>.Success(dtos);
    }

    public async Task<Result<AvailabilityBlockDto>> CreateBlockAsync(Guid therapistId, CreateBlockRequest request)
    {
        var block = new AvailabilityBlock
        {
            TherapistId = therapistId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Reason = request.Reason,
            IsRecurrence = request.IsRecurrence,
            RecurrenceRule = request.RecurrenceRule
        };

        var created = await _availabilityRepository.AddBlockAsync(block);

        var dto = new AvailabilityBlockDto(
            created.Id,
            created.StartAt,
            created.EndAt,
            created.Reason,
            created.IsRecurrence,
            created.RecurrenceRule);

        return Result<AvailabilityBlockDto>.Success(dto);
    }

    public async Task<Result> DeleteBlockAsync(Guid blockId)
    {
        await _availabilityRepository.RemoveBlockAsync(blockId);
        return Result.Success();
    }
}
