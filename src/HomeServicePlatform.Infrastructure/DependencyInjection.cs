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
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Infrastructure.ThirdPartyServices.Storage;

namespace HomeServicePlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IPaymentStrategy, WalletPaymentStrategy>();
            services.AddScoped<IPaymentStrategy, CashPaymentStrategy>();
            services.AddScoped<IPaymentStrategy, MockMoMoPaymentStrategy>();
            services.AddScoped<IPaymentStrategy, MockZaloPayPaymentStrategy>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<CloudinaryOptions>(opts =>
            {
                var section = configuration.GetSection(CloudinaryOptions.SectionName);
                opts.CloudName = section["CloudName"] ?? string.Empty;
                opts.ApiKey = section["ApiKey"] ?? string.Empty;
                opts.ApiSecret = section["ApiSecret"] ?? string.Empty;
            });
            services.AddSingleton<IImageStorage, CloudinaryImageStorage>();


            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));



            return services;
        }
    }
}
