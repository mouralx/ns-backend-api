using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Interfaces;
using MassageBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Infrastructure.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly AppDbContext _context;

    public AvailabilityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AvailabilityRule>> GetRulesAsync(Guid therapistId, int dayOfWeek)
    {
        return await _context.AvailabilityRules
            .Where(r => r.TherapistId == therapistId && r.DayOfWeek == dayOfWeek && r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<AvailabilityRule>> GetAllRulesAsync()
    {
        return await _context.AvailabilityRules.ToListAsync();
    }

    public async Task<AvailabilityRule> AddRuleAsync(AvailabilityRule rule)
    {
        await _context.AvailabilityRules.AddAsync(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task RemoveRuleAsync(Guid ruleId)
    {
        var rule = await _context.AvailabilityRules.FindAsync(ruleId);
        if (rule != null)
        {
            _context.AvailabilityRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AvailabilityBlock>> GetBlocksAsync(Guid therapistId)
    {
        return await _context.AvailabilityBlocks
            .Where(b => b.TherapistId == therapistId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AvailabilityBlock>> GetBlocksForDateAsync(Guid therapistId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = date.Date.AddDays(1);

        return await _context.AvailabilityBlocks
            .Where(b => b.TherapistId == therapistId &&
                       b.StartAt < dayEnd && b.EndAt > dayStart)
            .ToListAsync();
    }

    public async Task<AvailabilityBlock> AddBlockAsync(AvailabilityBlock block)
    {
        await _context.AvailabilityBlocks.AddAsync(block);
        await _context.SaveChangesAsync();
        return block;
    }

    public async Task RemoveBlockAsync(Guid blockId)
    {
        var block = await _context.AvailabilityBlocks.FindAsync(blockId);
        if (block != null)
        {
            _context.AvailabilityBlocks.Remove(block);
            await _context.SaveChangesAsync();
        }
    }
}
