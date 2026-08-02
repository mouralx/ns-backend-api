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
    private readonly IServiceTypeRepository _serviceTypeRepository;

    public ClientService(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        IServiceTypeRepository serviceTypeRepository)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _serviceTypeRepository = serviceTypeRepository;
    }

    public async Task<Result<IEnumerable<ClientDto>>> ListAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var clients = users.Where(u => u.Role == UserRole.Client);

        var dtos = clients.Select(c => new ClientDto(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.CreatedAt)).ToList();

        return Result<IEnumerable<ClientDto>>.Success(dtos);
    }

    public async Task<Result<ClientDetailDto>> GetDetailAsync(Guid clientId)
    {
        var client = await _userRepository.GetByIdAsync(clientId);
        if (client == null || client.Role != UserRole.Client)
        {
            return Result<ClientDetailDto>.Failure("Client not found");
        }

        var appointments = await _appointmentRepository.GetByClientAsync(clientId);
        var totalAppointments = appointments.Count();
        var lastVisit = appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.ScheduledAt)
            .FirstOrDefault()?.ScheduledAt;

        var dto = new ClientDetailDto(
            client.Id,
            client.Name,
            client.Email,
            client.Phone,
            client.CreatedAt,
            totalAppointments,
            lastVisit);

        return Result<ClientDetailDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetAppointmentsAsync(Guid clientId)
    {
        var appointments = await _appointmentRepository.GetByClientAsync(clientId);

        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            var therapist = await _userRepository.GetByIdAsync(appointment.TherapistId);
            var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);
            var client = await _userRepository.GetByIdAsync(appointment.ClientId);

            dtos.Add(new AppointmentDto(
                appointment.Id,
                appointment.ClientId,
                client != null ? new UserDto(client.Id, client.Email, client.Phone, client.Name, client.Role, client.IsActive, client.CreatedAt) : null!,
                appointment.TherapistId,
                therapist != null ? new UserDto(therapist.Id, therapist.Email, therapist.Phone, therapist.Name, therapist.Role, therapist.IsActive, therapist.CreatedAt) : null!,
                appointment.ServiceTypeId,
                serviceType != null ? new ServiceTypeDto(serviceType.Id, serviceType.Name, serviceType.DurationMin, serviceType.Description, serviceType.IsActive) : null!,
                appointment.ScheduledAt,
                appointment.DurationMin,
                appointment.Status,
                appointment.ConfirmationStatus,
                appointment.ConfirmedAt,
                appointment.CancelledAt,
                appointment.CancellationReason,
                appointment.Notes,
                appointment.IsWalkin,
                appointment.CreatedAt));
        }

        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }
}
