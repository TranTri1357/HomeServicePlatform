using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Infrastructure.Identity;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HomeServicePlatform.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            // Đọc cấu hình JWT
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"]!;

            // Đăng ký hệ thống Authentication của ASP.NET Core
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    // Sự kiện khi bị lỗi 401
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = new ApiResponse<object>
                        {
                            Succeeded = false,
                            StatusCode = 401,
                            Message = "Bạn chưa đăng nhập hoặc Token không hợp lệ/đã hết hạn."
                        };

                        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        await context.Response.WriteAsync(json);
                    },

                    // Sự kiện khi bị lỗi 403
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var response = new ApiResponse<object>
                        {
                            Succeeded = false,
                            StatusCode = 403,
                            Message = "Bạn không có quyền truy cập vào chức năng này."
                        };

                        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        await context.Response.WriteAsync(json);
                    },

                    // SignalR (WebSocket) không gửi được header Authorization,
                    // nên lấy JWT từ query string ?access_token=... cho các Hub.
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chat-hub") || path.StartsWithSegments("/booking-hub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Tắt kiểm tra ModelState mặc định, để FluentValidation và Middleware
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            //services.AddSwaggerGen(); // Cấu hình Swagger để test API
            services.AddSwaggerGen(c =>
            {
                // 1. Định nghĩa giao diện nút Authorize
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Vui lòng nhập Token theo định dạng: Bearer {chuỗi_token_của_bạn}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // 2. Yêu cầu Swagger đính kèm cái Token đó vào mỗi Request
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Đăng ký SignalR Hub nếu dùng
            services.AddSignalR();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly);
            });

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}
