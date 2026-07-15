
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Api.Middlewares;
using HomeServicePlatform.Application;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Infrastructure;
using HomeServicePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //Đăng kí cấu hình API
            builder.Services.AddApiServices(builder.Configuration);
            //Đăng kí cấu hình Application
            builder.Services.AddApplicationServices();
            // Đăng kí cấu hình Infrastructure
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Chính sách hủy/hoàn tiền — Admin chỉnh qua section "RefundPolicy" trong appsettings.
            builder.Services.Configure<RefundPolicyOptions>(
                builder.Configuration.GetSection(RefundPolicyOptions.SectionName));

            // Chính sách buffer time di chuyển giữa 2 đơn — section "BufferPolicy".
            builder.Services.Configure<BufferPolicyOptions>(
                builder.Configuration.GetSection(BufferPolicyOptions.SectionName));

            var app = builder.Build();

            // KÍCH HOẠT CÁI LƯỚI BẮT LỖI TOÀN HỆ THỐNG
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
                app.UseSwagger();
                app.UseSwaggerUI();
            

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            // Đặt đường dẫn để Client kết nối tới  /booking-hub
            app.MapHub<BookingHub>("/booking-hub");
            // Hub chat theo đơn
            app.MapHub<ChatHub>("/chat-hub");

            app.Run();
        }
    }
}
