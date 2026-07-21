
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
            builder.Services.AddApiServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.Configure<RefundPolicyOptions>(
                builder.Configuration.GetSection(RefundPolicyOptions.SectionName));

            builder.Services.Configure<BufferPolicyOptions>(
                builder.Configuration.GetSection(BufferPolicyOptions.SectionName));

            builder.Services.Configure<BookingPolicyOptions>(
                builder.Configuration.GetSection(BookingPolicyOptions.SectionName));

            var app = builder.Build();

            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseOutputCache();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.MapHub<BookingHub>("/booking-hub");
            app.MapHub<ChatHub>("/chat-hub");

            app.MapMethods(
    "/health",
    new[] { "GET", "HEAD" },
    () => Results.Ok(new { status = "ok" }));

            app.Run();
        }
    }
}
