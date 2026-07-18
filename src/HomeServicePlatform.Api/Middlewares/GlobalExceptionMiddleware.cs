using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Responses;
using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeServicePlatform.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next, IHostEnvironment env, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // Nếu có lỗi ở bất kỳ đâu, nhảy vào đây bắt lại
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError; // Mặc định là lỗi 500
            var apiResponse = new ApiResponse<object> { Succeeded = false };

            switch (exception)
            {
                // 1. Nếu là lỗi dữ liệu đầu vào (Do cái ValidationBehavior ném ra)
                case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = "Dữ liệu đầu vào không hợp lệ.";
                apiResponse.Errors = valEx.Errors.Select(e => e.ErrorMessage).ToList();
                break;

                // 2. Nếu là lỗi không tìm thấy dữ liệu
                case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = notFoundEx.Message;
                break;

                // 3. Nếu là lỗi yêu cầu sai nghiệp vụ
                case BadRequestException badReqEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = badReqEx.Message;
                break;

                //Bắt lỗi 401 Unauthorized
                case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = unauthorizedEx.Message;
                break;

                //Bắt lỗi 403 Forbidden
                case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = forbiddenEx.Message;
                break;

                // 4. Các lỗi hệ thống không lường trước được (Lỗi sập nguồn, NullReference...)
                default:
                statusCode = HttpStatusCode.InternalServerError;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = "Đã xảy ra lỗi hệ thống nghiêm trọng. Vui lòng thử lại sau.";
                // 🔒 Luôn log đầy đủ ở server; CHỈ lộ chi tiết lỗi cho client khi chạy Development.
                _logger.LogError(exception, "Lỗi hệ thống không xử lý được.");
                if (_env.IsDevelopment())
                    apiResponse.Errors = new List<string> { exception.Message };
                break;
            }

            context.Response.StatusCode = (int)statusCode;
            var jsonResult = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
