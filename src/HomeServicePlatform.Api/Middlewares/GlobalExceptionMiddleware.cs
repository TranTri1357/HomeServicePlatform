using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Responses;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace HomeServicePlatform.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate _next)
        {
            this._next = _next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Cho request đi tiếp bình thường
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // Nếu có lỗi ở bất kỳ đâu, nhảy vào đây bắt lại
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError; // Mặc định là lỗi 500
            var apiResponse = new ApiResponse<object> { Succeeded = false };

            switch (exception)
            {
                // 1. Nếu là lỗi dữ liệu đầu vào (Do cái ValidationBehavior ném ra)
                case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.Message = "Dữ liệu đầu vào không hợp lệ.";
                apiResponse.Errors = valEx.Errors.Select(e => e.ErrorMessage).ToList();
                break;

                // 2. Nếu là lỗi không tìm thấy dữ liệu (Do mình tự throw ở Handler)
                case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                apiResponse.Message = notFoundEx.Message;
                break;

                // 3. Nếu là lỗi yêu cầu sai nghiệp vụ
                case BadRequestException badReqEx:
                statusCode = HttpStatusCode.BadRequest;
                apiResponse.Message = badReqEx.Message;
                break;

                // 4. Các lỗi hệ thống không lường trước được (Lỗi sập nguồn, NullReference...)
                default:
                statusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = "Đã xảy ra lỗi hệ thống nghiêm trọng. Vui lòng thử lại sau.";
                apiResponse.Errors = new List<string> { exception.Message }; // Chỉ dùng khi dev, production nên ẩn đi
                break;
            }

            context.Response.StatusCode = (int)statusCode;
            var jsonResult = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(jsonResult);
        }
    }
}
