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

    // Rules
    public async Task<IEnumerable<AvailabilityRule>> GetAllRulesAsync()
    {
        return await _context.AvailabilityRules.ToListAsync();
    }

    public async Task<IEnumerable<AvailabilityRule>> GetRulesAsync(Guid therapistId, int dayOfWeek)
    {
        return await _context.AvailabilityRules
            .Where(r => r.TherapistId == therapistId && r.DayOfWeek == dayOfWeek)
            .ToListAsync();
    }

    public async Task<AvailabilityRule?> GetRuleByIdAsync(Guid id)
    {
        return await _context.AvailabilityRules.FindAsync(id);
    }

    public async Task<AvailabilityRule> AddRuleAsync(AvailabilityRule rule)
    {
        _context.AvailabilityRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task UpdateRuleAsync(AvailabilityRule rule)
    {
        _context.AvailabilityRules.Update(rule);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRuleAsync(Guid id)
    {
        var rule = await _context.AvailabilityRules.FindAsync(id);
        if (rule != null)
        {
            _context.AvailabilityRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    // Blocks
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
            .Where(b => b.TherapistId == therapistId
                && b.StartAt < dayEnd && b.EndAt > dayStart)
            .ToListAsync();
    }

    public async Task<AvailabilityBlock?> GetBlockByIdAsync(Guid id)
    {
        return await _context.AvailabilityBlocks.FindAsync(id);
    }

    public async Task<AvailabilityBlock> AddBlockAsync(AvailabilityBlock block)
    {
        _context.AvailabilityBlocks.Add(block);
        await _context.SaveChangesAsync();
        return block;
    }

    public async Task UpdateBlockAsync(AvailabilityBlock block)
    {
        _context.AvailabilityBlocks.Update(block);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveBlockAsync(Guid id)
    {
        var block = await _context.AvailabilityBlocks.FindAsync(id);
        if (block != null)
        {
            _context.AvailabilityBlocks.Remove(block);
            await _context.SaveChangesAsync();
        }
    }
}
