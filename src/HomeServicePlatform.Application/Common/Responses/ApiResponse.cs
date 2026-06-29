using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Responses
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; } 
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Hàm nhanh trả về thành công (Mặc định HTTP 200 - OK)
        public static ApiResponse<T> Success(T data, string message = "Thành công", int statusCode = 200)
            => new()
            {
                Succeeded = true,
                StatusCode = statusCode,
                Data = data,
                Message = message
            };

        // Hàm nhanh trả về thất bại với danh sách lỗi (Mặc định HTTP 400 - Bad Request)
        public static ApiResponse<T> Failure(List<string> errors, string message = "Có lỗi xảy ra", int statusCode = 400)
            => new()
            {
                Succeeded = false,
                StatusCode = statusCode,
                Errors = errors,
                Message = message
            };

        // Hàm nhanh trả về thất bại với 1 lỗi duy nhất (Mặc định HTTP 400 - Bad Request)
        public static ApiResponse<T> Failure(string error, string message = "Có lỗi xảy ra", int statusCode = 400)
            => new()
            {
                Succeeded = false,
                StatusCode = statusCode,
                Errors = new List<string> { error },
                Message = message
            };
    }
}
