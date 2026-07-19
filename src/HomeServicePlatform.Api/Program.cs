
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Api.Middlewares;
using HomeServicePlatform.Application;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Infrastructure;
using HomeServicePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
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

            // Công tắc nghiệp vụ luồng đặt lịch (vd ép giờ làm việc) — section "BookingPolicy".
            builder.Services.Configure<BookingPolicyOptions>(
                builder.Configuration.GetSection(BookingPolicyOptions.SectionName));

            var app = builder.Build();

            // 🌐 PHẢI ĐỨNG ĐẦU PIPELINE: khôi phục IP thật của client từ header X-Forwarded-For.
            //
            // Trên Render (và mọi PaaS khác) ứng dụng nằm SAU reverse proxy, nên
            // HttpContext.Connection.RemoteIpAddress là IP của proxy — GIỐNG NHAU cho mọi người
            // dùng. Rate limiter phân vùng theo IP đó ⇒ hạn mức 10 request/phút bị áp cho TOÀN BỘ
            // người dùng cộng lại, chỉ vài người thao tác cùng lúc là cả hệ thống nhận 429.
            //
            // KnownNetworks/KnownProxies phải xoá vì dải IP proxy của Render không cố định;
            // không xoá thì ASP.NET bỏ qua header và mọi thứ trở lại như cũ.
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            // KÍCH HOẠT CÁI LƯỚI BẮT LỖI TOÀN HỆ THỐNG
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            // Swagger chỉ bật ở Development, hoặc khi bật cờ "EnableSwagger" (để test tạm trên server).
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseOutputCache(); // phải đứng sau UseCors, trước khi map endpoint
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            // Đặt đường dẫn để Client kết nối tới  /booking-hub
            app.MapHub<BookingHub>("/booking-hub");
            // Hub chat theo đơn
            app.MapHub<ChatHub>("/chat-hub");

            app.MapMethods(
    "/health",
    new[] { "GET", "HEAD" },
    () => Results.Ok(new { status = "ok" }));

            app.Run();
        }
    }
}
