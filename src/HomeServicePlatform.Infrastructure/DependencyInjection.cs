using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using HomeServicePlatform.Infrastructure.Identity;
using HomeServicePlatform.Infrastructure.Persistence;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies;

namespace HomeServicePlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Lấy chuỗi kết nối từ Configuration giống hệt bên Program
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Cấu hình DbContext kết hợp PostgreSQL + PostGIS (NetTopologySuite)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Đăng ký các Repository đặc thù khác nếu có
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IPaymentStrategy, WalletPaymentStrategy>();
            //services.AddScoped<IPaymentStrategy, MoMoPaymentStrategy>();
            //services.AddScoped<IPaymentStrategy, ZaloPayPaymentStrategy>();
            services.AddScoped<IPaymentStrategy, CashPaymentStrategy>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


            services.AddScoped<IUnitOfWork, UnitOfWork>();


            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));



            return services;
        }
    }
}
