using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings;

namespace HomeServicePlatform.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(); // Cấu hình Swagger để test API

            // Đăng ký SignalR Hub nếu dùng
            services.AddSignalR();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly);
            });
            return services;
        }
    }
}
