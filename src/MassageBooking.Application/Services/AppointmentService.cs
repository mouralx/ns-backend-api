using MassageBooking.Application.DTOs;
using MassageBooking.Application.Interfaces;
using MassageBooking.Domain.Common;
using MassageBooking.Domain.Entities;
using MassageBooking.Domain.Enums;
using MassageBooking.Domain.Interfaces;

namespace MassageBooking.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IServiceTypeRepository _serviceTypeRepository;
    private readonly IAvailabilityRepository _availabilityRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IServiceTypeRepository serviceTypeRepository,
        IAvailabilityRepository availabilityRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<AppointmentDto>> GetByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return Result<AppointmentDto>.Failure("Appointment not found");
        }

        return Result<AppointmentDto>.Success(await MapToDtoAsync(appointment));
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetAllAsync()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            dtos.Add(await MapToDtoAsync(appointment));
        }
        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetByClientAsync(Guid clientId)
    {
        var appointments = await _appointmentRepository.GetByClientAsync(clientId);
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            dtos.Add(await MapToDtoAsync(appointment));
        }
        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetByTherapistAsync(Guid therapistId)
    {
        var appointments = await _appointmentRepository.GetByTherapistAsync(therapistId);
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            dtos.Add(await MapToDtoAsync(appointment));
        }
        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    public async Task<Result<AppointmentDto>> BookAsync(BookAppointmentRequest request, Guid clientId)
    {
        // Validate therapist exists
        var therapist = await _userRepository.GetByIdAsync(request.TherapistId);
        if (therapist == null || therapist.Role != UserRole.Therapist)
        {
            return Result<AppointmentDto>.Failure("Invalid therapist");
        }

        // Validate service type exists
        var serviceType = await _serviceTypeRepository.GetByIdAsync(request.ServiceTypeId);
        if (serviceType == null || !serviceType.IsActive)
        {
            return Result<AppointmentDto>.Failure("Invalid service type");
        }

        // Check slot availability
        var endAt = request.ScheduledAt.AddMinutes(serviceType.DurationMin);
        var conflicts = await _appointmentRepository.GetConflictingAsync(
            request.TherapistId, request.ScheduledAt, endAt);

        if (conflicts.Any())
        {
            return Result<AppointmentDto>.Failure("Time slot is not available");
        }

        var appointment = new Appointment
        {
            ClientId = clientId,
            TherapistId = request.TherapistId,
            ServiceTypeId = request.ServiceTypeId,
            ScheduledAt = request.ScheduledAt,
            DurationMin = serviceType.DurationMin,
            Status = AppointmentStatus.Pending,
            ConfirmationStatus = ConfirmationStatus.Unconfirmed,
            Notes = request.Notes,
            IsWalkin = false
        };

        await _appointmentRepository.AddAsync(appointment);

        // Reload with navigation properties
        var saved = await _appointmentRepository.GetByIdAsync(appointment.Id);
        return Result<AppointmentDto>.Success(await MapToDtoAsync(saved!));
    }

    public async Task<Result<AppointmentDto>> CancelAsync(Guid id, string? reason = null)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return Result<AppointmentDto>.Failure("Appointment not found");
        }

        // Check 4-hour cancellation rule for clients
        var hoursUntil = (appointment.ScheduledAt - DateTime.UtcNow).TotalHours;
        if (hoursUntil < 4)
        {
            return Result<AppointmentDto>.Failure("Cannot cancel within 4 hours of appointment");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = reason;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        return Result<AppointmentDto>.Success(await MapToDtoAsync(appointment));
    }

    public async Task<Result<AppointmentDto>> ConfirmAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return Result<AppointmentDto>.Failure("Appointment not found");
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ConfirmationStatus = ConfirmationStatus.Confirmed;
        appointment.ConfirmedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        return Result<AppointmentDto>.Success(await MapToDtoAsync(appointment));
    }

    public async Task<Result<SlotsResponse>> GetAvailableSlotsAsync(
        Guid therapistId, Guid serviceTypeId, DateTime date)
    {
        var serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId);
        if (serviceType == null)
        {
            return Result<SlotsResponse>.Failure("Service type not found");
        }

        // Get availability rules for the day
        var dayOfWeek = (int)date.DayOfWeek;
        var rules = await _availabilityRepository.GetRulesAsync(therapistId, dayOfWeek);

        if (!rules.Any())
        {
            return Result<SlotsResponse>.Success(new SlotsResponse(
                date.Date, therapistId, serviceTypeId, serviceType.DurationMin, new List<SlotDto>()));
        }

        // Get blocks for the day
        var blocks = await _availabilityRepository.GetBlocksForDateAsync(therapistId, date);

        // Get existing appointments for the day
        var dayStart = date.Date;
        var dayEnd = date.Date.AddDays(1);
        var appointments = await _appointmentRepository.GetByDateRangeAsync(dayStart, dayEnd);
        var therapistAppointments = appointments.Where(a =>
            a.TherapistId == therapistId &&
            a.Status != AppointmentStatus.Cancelled);

        // Calculate available slots
        var slots = new List<SlotDto>();

        foreach (var rule in rules.Where(r => r.IsActive))
        {
            var currentTime = date.Date.Add(rule.StartTime.ToTimeSpan());
            var endTime = date.Date.Add(rule.EndTime.ToTimeSpan());

            while (currentTime.AddMinutes(serviceType.DurationMin) <= endTime)
            {
                var slotEnd = currentTime.AddMinutes(serviceType.DurationMin);

                // Check if slot conflicts with blocks
                var blockedByBlock = blocks.Any(b =>
                    currentTime < b.EndAt && slotEnd > b.StartAt);

                // Check if slot conflicts with appointments
                var blockedByAppointment = therapistAppointments.Any(a =>
                    currentTime < a.ScheduledAt.AddMinutes(a.DurationMin) &&
                    slotEnd > a.ScheduledAt);

                if (!blockedByBlock && !blockedByAppointment)
                {
                    slots.Add(new SlotDto(currentTime, slotEnd));
                }

                currentTime = currentTime.AddMinutes(30); // 30-minute intervals
            }
        }

        return Result<SlotsResponse>.Success(new SlotsResponse(
            date.Date, therapistId, serviceTypeId, serviceType.DurationMin, slots));
    }

    private async Task<AppointmentDto> MapToDtoAsync(Appointment appointment)
    {
        var client = await _userRepository.GetByIdAsync(appointment.ClientId);
        var therapist = await _userRepository.GetByIdAsync(appointment.TherapistId);
        var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);

        return new AppointmentDto(
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
            appointment.CreatedAt);
    }
}
