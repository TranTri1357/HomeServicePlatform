using HomeServicePlatform.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace HomeServicePlatform.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Đăng ký MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Đăng ký tự động tất cả các file Validator (FluentValidation) có trong tầng này
            services.AddValidatorsFromAssembly(assembly);

            // ⚠️ THỨ TỰ ĐĂNG KÝ = THỨ TỰ CHẠY. ConcurrencyRetryBehavior phải đứng NGOÀI CÙNG để
            // khi chạy lại thì chạy lại trọn vẹn cả lệnh (gồm cả bước validate), chứ không phải
            // chỉ mỗi phần thân handler.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyRetryBehavior<,>));

            // Đăng ký cái ValidationBehavior tự động chạy chung với MediatR
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
