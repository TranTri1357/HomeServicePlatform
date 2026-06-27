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
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Hàm nhanh để trả về kết quả thành công
        public static ApiResponse<T> Success(T data, string message = "Thành công")
            => new() { Succeeded = true, Data = data, Message = message };

        // Hàm nhanh để trả về kết quả thất bại
        public static ApiResponse<T> Failure(List<string> errors, string message = "Có lỗi xảy ra")
            => new() { Succeeded = false, Errors = errors, Message = message };

        public static ApiResponse<T> Failure(string error, string message = "Có lỗi xảy ra")
            => new() { Succeeded = false, Errors = new List<string> { error }, Message = message };
    }
}
