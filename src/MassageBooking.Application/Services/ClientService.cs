using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class ClientService : IClientService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public ClientService(IUserRepository userRepository, IAppointmentRepository appointmentRepository)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<IEnumerable<ClientDto>>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var clients = users.Where(u => u.Role == UserRole.Client);

        var dtos = clients.Select(u => new ClientDto(
            u.Id,
            u.Email,
            u.Phone,
            u.Name,
            u.IsActive,
            u.CreatedAt)).ToList();

        return Result<IEnumerable<ClientDto>>.Success(dtos);
    }

    public async Task<Result<ClientDto>> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Client)
        {
            return Result<ClientDto>.Failure("Client not found");
        }

        var dto = new ClientDto(
            user.Id,
            user.Email,
            user.Phone,
            user.Name,
            user.IsActive,
            user.CreatedAt);

        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<ClientDto>> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Client)
        {
            return Result<ClientDto>.Failure("Client not found");
        }

        if (request.Phone != null) user.Phone = request.Phone;
        if (request.Name != null) user.Name = request.Name;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var dto = new ClientDto(
            user.Id,
            user.Email,
            user.Phone,
            user.Name,
            user.IsActive,
            user.CreatedAt);

        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetAppointmentHistoryAsync(Guid clientId)
    {
        var client = await _userRepository.GetByIdAsync(clientId);
        if (client == null || client.Role != UserRole.Client)
        {
            return Result<IEnumerable<AppointmentDto>>.Failure("Client not found");
        }

        var appointments = await _appointmentRepository.GetByClientAsync(clientId);
        var dtos = appointments.Select(a => new AppointmentDto(
            a.Id,
            a.ClientId,
            null!,
            a.TherapistId,
            null!,
            a.ServiceTypeId,
            null!,
            a.ScheduledAt,
            a.DurationMin,
            a.Status,
            a.ConfirmationStatus,
            a.ConfirmedAt,
            a.CancelledAt,
            a.CancellationReason,
            a.Notes,
            a.IsWalkin,
            a.CreatedAt)).ToList();

        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }
}
