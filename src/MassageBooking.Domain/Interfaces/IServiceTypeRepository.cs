using MassageBooking.Domain.Entities;

namespace MassageBooking.Domain.Interfaces;

public interface IServiceTypeRepository
{
    Task<IEnumerable<ServiceType>> GetAllAsync();
    Task<IEnumerable<ServiceType>> GetActiveAsync();
    Task<ServiceType?> GetByIdAsync(Guid id);
    Task<ServiceType> AddAsync(ServiceType serviceType);
    Task<ServiceType> UpdateAsync(ServiceType serviceType);
    Task DeleteAsync(Guid id);
}
