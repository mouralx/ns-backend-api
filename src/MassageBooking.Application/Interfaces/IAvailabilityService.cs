using MassageBooking.Application.DTOs;
using MassageBooking.Domain.Common;

namespace MassageBooking.Application.Interfaces;

public interface IAvailabilityService
{
    Task<Result<IEnumerable<AvailabilityRuleDto>>> GetRulesAsync(Guid therapistId);
    Task<Result<AvailabilityRuleDto>> CreateRuleAsync(Guid therapistId, CreateRuleRequest request);
    Task<Result<AvailabilityRuleDto>> UpdateRuleAsync(Guid therapistId, Guid ruleId, UpdateRuleRequest request);
    Task<Result> DeleteRuleAsync(Guid ruleId);
    Task<Result<IEnumerable<AvailabilityBlockDto>>> GetBlocksAsync(Guid therapistId);
    Task<Result<AvailabilityBlockDto>> CreateBlockAsync(Guid therapistId, CreateBlockRequest request);
    Task<Result<AvailabilityBlockDto>> UpdateBlockAsync(Guid therapistId, Guid blockId, UpdateBlockRequest request);
    Task<Result> DeleteBlockAsync(Guid blockId);
}
