using MassageBooking.Application.Interfaces;
using MassageBooking.Application.Services;
using MassageBooking.Domain.Interfaces;
using MassageBooking.Infrastructure.Data;
using MassageBooking.Infrastructure.Repositories;
using MassageBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MassageBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddHttpClient<IPushNotificationService, PushNotificationService>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IClientService, ClientService>();

        return services;
    }
}
