using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Interfaces;
using MassageBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Infrastructure.Repositories;

public class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly AppDbContext _context;

    public ServiceTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceType>> GetAllAsync()
    {
        return await _context.ServiceTypes.ToListAsync();
    }

    public async Task<IEnumerable<ServiceType>> GetActiveAsync()
    {
        return await _context.ServiceTypes
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<ServiceType?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceTypes.FindAsync(id);
    }

    public async Task<ServiceType> AddAsync(ServiceType serviceType)
    {
        await _context.ServiceTypes.AddAsync(serviceType);
        await _context.SaveChangesAsync();
        return serviceType;
    }

    public async Task<ServiceType> UpdateAsync(ServiceType serviceType)
    {
        _context.ServiceTypes.Update(serviceType);
        await _context.SaveChangesAsync();
        return serviceType;
    }

    public async Task DeleteAsync(Guid id)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType != null)
        {
            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();
        }
    }
}
