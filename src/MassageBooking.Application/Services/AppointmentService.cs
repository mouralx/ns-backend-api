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
    private readonly INotificationService _notificationService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IServiceTypeRepository serviceTypeRepository,
        IAvailabilityRepository availabilityRepository,
        INotificationService notificationService)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _availabilityRepository = availabilityRepository;
        _notificationService = notificationService;
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
        var therapist = await _userRepository.GetByIdAsync(request.TherapistId);
        if (therapist == null || therapist.Role != UserRole.Therapist)
        {
            return Result<AppointmentDto>.Failure("Invalid therapist");
        }

        var serviceType = await _serviceTypeRepository.GetByIdAsync(request.ServiceTypeId);
        if (serviceType == null || !serviceType.IsActive)
        {
            return Result<AppointmentDto>.Failure("Invalid service type");
        }

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
            Status = AppointmentStatus.Scheduled,
            ConfirmationStatus = ConfirmationStatus.Pending,
            Notes = request.Notes,
            IsWalkin = false
        };

        await _appointmentRepository.AddAsync(appointment);

        var saved = await _appointmentRepository.GetByIdAsync(appointment.Id);
        var dto = await MapToDtoAsync(saved!);

        // Send notification to therapist about new booking
        await _notificationService.CreateBookingNotificationAsync(
            appointment.Id, appointment.ClientId, appointment.TherapistId, serviceType.Name, appointment.ScheduledAt);

        return Result<AppointmentDto>.Success(dto);
    }

    public async Task<Result<AppointmentDto>> UpdateAsync(Guid id, UpdateAppointmentRequest request)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return Result<AppointmentDto>.Failure("Appointment not found");
        }

        if (appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.Completed)
        {
            return Result<AppointmentDto>.Failure("Cannot update cancelled or completed appointment");
        }

        if (request.ScheduledAt.HasValue)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);
            var endAt = request.ScheduledAt.Value.AddMinutes(serviceType?.DurationMin ?? appointment.DurationMin);
            var conflicts = await _appointmentRepository.GetConflictingAsync(
                appointment.TherapistId, request.ScheduledAt.Value, endAt);

            // Exclude current appointment from conflict check
            conflicts = conflicts.Where(c => c.Id != id);

            if (conflicts.Any())
            {
                return Result<AppointmentDto>.Failure("New time slot is not available");
            }

            appointment.ScheduledAt = request.ScheduledAt.Value;
            appointment.DurationMin = serviceType?.DurationMin ?? appointment.DurationMin;
        }

        if (request.Notes != null)
        {
            appointment.Notes = request.Notes;
        }

        appointment.UpdatedAt = DateTime.UtcNow;
        await _appointmentRepository.UpdateAsync(appointment);

        return Result<AppointmentDto>.Success(await MapToDtoAsync(appointment));
    }

    public async Task<Result<AppointmentDto>> CancelAsync(Guid id, string? reason = null)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return Result<AppointmentDto>.Failure("Appointment not found");
        }

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

        // Send cancellation notification
        await _notificationService.CreateCancellationNotificationAsync(
            appointment.Id, appointment.ClientId, appointment.TherapistId);

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

        // Send confirmation notification to therapist
        await _notificationService.CreateConfirmedNotificationAsync(
            appointment.Id, appointment.ClientId, appointment.TherapistId);

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

        var dayOfWeek = (int)date.DayOfWeek;
        var rules = await _availabilityRepository.GetRulesAsync(therapistId, dayOfWeek);

        if (!rules.Any())
        {
            return Result<SlotsResponse>.Success(new SlotsResponse(
                date.Date, therapistId, serviceTypeId, serviceType.DurationMin, new List<SlotDto>()));
        }

        var blocks = await _availabilityRepository.GetBlocksForDateAsync(therapistId, date);

        var dayStart = date.Date;
        var dayEnd = date.Date.AddDays(1);
        var appointments = await _appointmentRepository.GetByDateRangeAsync(dayStart, dayEnd);
        var therapistAppointments = appointments.Where(a =>
            a.TherapistId == therapistId &&
            a.Status != AppointmentStatus.Cancelled);

        var slots = new List<SlotDto>();

        foreach (var rule in rules.Where(r => r.IsActive))
        {
            var currentTime = date.Date.Add(rule.StartTime.ToTimeSpan());
            var endTime = date.Date.Add(rule.EndTime.ToTimeSpan());

            while (currentTime.AddMinutes(serviceType.DurationMin) <= endTime)
            {
                var slotEnd = currentTime.AddMinutes(serviceType.DurationMin);

                var blockedByBlock = blocks.Any(b =>
                    currentTime < b.EndAt && slotEnd > b.StartAt);

                var blockedByAppointment = therapistAppointments.Any(a =>
                    currentTime < a.ScheduledAt.AddMinutes(a.DurationMin) &&
                    slotEnd > a.ScheduledAt);

                if (!blockedByBlock && !blockedByAppointment)
                {
                    slots.Add(new SlotDto(currentTime, slotEnd));
                }

                currentTime = currentTime.AddMinutes(30);
            }
        }

        return Result<SlotsResponse>.Success(new SlotsResponse(
            date.Date, therapistId, serviceTypeId, serviceType.DurationMin, slots));
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> GetUpcomingAsync(Guid? therapistId = null, int limit = 10)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddDays(30);

        var appointments = await _appointmentRepository.GetByDateRangeAsync(now, futureDate);

        if (therapistId.HasValue)
        {
            appointments = appointments.Where(a => a.TherapistId == therapistId.Value);
        }

        appointments = appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
            .OrderBy(a => a.ScheduledAt)
            .Take(limit);

        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            dtos.Add(await MapToDtoAsync(appointment));
        }
        return Result<IEnumerable<AppointmentDto>>.Success(dtos);
    }

    private async Task<AppointmentDto> MapToDtoAsync(Appointment appointment)
    {
        var client = await _userRepository.GetByIdAsync(appointment.ClientId);
        var therapist = await _userRepository.GetByIdAsync(appointment.TherapistId);
        var serviceType = await _serviceTypeRepository.GetByIdAsync(appointment.ServiceTypeId);

        return new AppointmentDto(
            appointment.Id,
            appointment.ClientId,
            client != null ? new UserDto(client.Id, client.Email, client.Phone, client.Name, client.Role, client.PushToken, client.IsActive, client.CreatedAt, client.UpdatedAt) : null!,
            appointment.TherapistId,
            therapist != null ? new UserDto(therapist.Id, therapist.Email, therapist.Phone, therapist.Name, therapist.Role, therapist.PushToken, therapist.IsActive, therapist.CreatedAt, therapist.UpdatedAt) : null!,
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