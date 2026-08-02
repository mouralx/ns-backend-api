using MassageBooking.Domain.Entities;

namespace MassageBooking.Domain.Interfaces;

public interface IAvailabilityRepository
{
    // Rules
    Task<IEnumerable<AvailabilityRule>> GetAllRulesAsync();
    Task<IEnumerable<AvailabilityRule>> GetRulesAsync(Guid therapistId, int dayOfWeek);
    Task<AvailabilityRule?> GetRuleByIdAsync(Guid id);
    Task<AvailabilityRule> AddRuleAsync(AvailabilityRule rule);
    Task UpdateRuleAsync(AvailabilityRule rule);
    Task RemoveRuleAsync(Guid id);

    // Blocks
    Task<IEnumerable<AvailabilityBlock>> GetBlocksAsync(Guid therapistId);
    Task<IEnumerable<AvailabilityBlock>> GetBlocksForDateAsync(Guid therapistId, DateTime date);
    Task<AvailabilityBlock?> GetBlockByIdAsync(Guid id);
    Task<AvailabilityBlock> AddBlockAsync(AvailabilityBlock block);
    Task UpdateBlockAsync(AvailabilityBlock block);
    Task RemoveBlockAsync(Guid id);
}
