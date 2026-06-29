using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Infrastructure.Persistence;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Lấy chuỗi kết nối từ Configuration giống hệt bên Program
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 2. Cấu hình DbContext kết hợp PostgreSQL + PostGIS (NetTopologySuite)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // 3. Đăng ký các Repository đặc thù khác nếu có (Ví dụ: BookingRepository...)
            // services.AddScoped<IBookingRepository, BookingRepository>();

            // Đăng ký Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Đăng ký Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }
    }
}
