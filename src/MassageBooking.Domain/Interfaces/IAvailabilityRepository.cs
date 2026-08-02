using MassageBooking.Domain.Entities;

namespace MassageBooking.Domain.Interfaces;

public interface IAvailabilityRepository
{
    Task<IEnumerable<AvailabilityRule>> GetRulesAsync(Guid therapistId, int dayOfWeek);
    Task<IEnumerable<AvailabilityRule>> GetAllRulesAsync();
    Task<AvailabilityRule> AddRuleAsync(AvailabilityRule rule);
    Task RemoveRuleAsync(Guid ruleId);
    Task<IEnumerable<AvailabilityBlock>> GetBlocksAsync(Guid therapistId);
    Task<IEnumerable<AvailabilityBlock>> GetBlocksForDateAsync(Guid therapistId, DateTime date);
    Task<AvailabilityBlock> AddBlockAsync(AvailabilityBlock block);
    Task RemoveBlockAsync(Guid blockId);
}
