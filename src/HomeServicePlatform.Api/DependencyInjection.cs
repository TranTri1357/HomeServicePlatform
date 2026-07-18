using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Infrastructure.Identity;
using HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace HomeServicePlatform.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 🌐 CORS: KHÔNG mở cho mọi origin nữa. Chỉ cho phép các origin frontend đã biết.
            //    Origin production cấu hình qua "Cors:AllowedOrigins" (appsettings/env),
            //    cộng thêm các origin dev localhost mặc định bên dưới.
            var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                    ?? Array.Empty<string>();
            var defaultDevOrigins = new[]
            {
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
            };
            var allowedOrigins = defaultDevOrigins.Concat(configuredOrigins)
                                                  .Where(o => !string.IsNullOrWhiteSpace(o))
                                                  .Distinct()
                                                  .ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            // Đọc cấu hình JWT
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];

            // 🔐 Secret KHÔNG còn nằm trong appsettings.json (đã gỡ). Bắt buộc nạp qua
            // User Secrets (dev) hoặc biến môi trường JwtSettings__Secret (production).
            // Fail-fast với thông báo rõ ràng thay vì lỗi khó hiểu khi validate token.
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtSettings:Secret chưa được cấu hình (hoặc quá ngắn < 32 ký tự). " +
                    "Đặt qua `dotnet user-secrets set \"JwtSettings:Secret\" \"...\"` khi chạy local, " +
                    "hoặc biến môi trường JwtSettings__Secret trên server. Xem README.");
            }

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
                    // Dung sai 30s cho lệch giờ nhẹ giữa client/server (tránh 401 lẻ tẻ khi token vừa hết hạn).
                    ClockSkew = TimeSpan.FromSeconds(30),
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

            // 🚦 Rate limiting: chống brute-force / spam ở các endpoint xác thực.
            //    Policy "auth" = 10 request/phút, phân vùng theo IP client.
            //    Lưu ý: sau reverse proxy (Render), RemoteIpAddress có thể là IP proxy →
            //    cân nhắc cấu hình ForwardedHeaders nếu muốn giới hạn chính xác theo IP thật.
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 10,
                            QueueLimit = 0
                        }));

                // Trả 429 theo đúng khuôn ApiResponse để frontend xử lý đồng nhất.
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    var payload = new ApiResponse<object>
                    {
                        Succeeded = false,
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        Message = "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút."
                    };
                    var json = JsonSerializer.Serialize(payload,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await context.HttpContext.Response.WriteAsync(json, token);
                };
            });

            // ⏱️ Job nền dọn đơn đặt lịch hết hạn (nhả slot). Thay cho đoạn quét nội tuyến
            //    trong CreateBooking trước đây.
            services.AddHostedService<BackgroundJobs.ExpiredBookingCleanupService>();

            // 🚀 Output caching cho các endpoint catalog công khai (ít thay đổi) — giảm tải DB.
            //    Từng endpoint tự khai [OutputCache(...)]; ở đây chỉ bật hạ tầng.
            services.AddOutputCache();

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
