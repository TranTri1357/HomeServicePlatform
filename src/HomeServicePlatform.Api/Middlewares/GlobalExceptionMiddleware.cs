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
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError;
            var apiResponse = new ApiResponse<object> { Succeeded = false };

            switch (exception)
            {
                case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = "Dữ liệu đầu vào không hợp lệ.";
                apiResponse.Errors = valEx.Errors.Select(e => e.ErrorMessage).ToList();
                break;

                case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = notFoundEx.Message;
                break;

                case BadRequestException badReqEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = badReqEx.Message;
                break;

                case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = unauthorizedEx.Message;
                break;

                case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = forbiddenEx.Message;
                break;

                default:
                statusCode = HttpStatusCode.InternalServerError;
                apiResponse.StatusCode = (int)statusCode;
                apiResponse.Message = "Đã xảy ra lỗi hệ thống nghiêm trọng. Vui lòng thử lại sau.";
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
