using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateCustomerProfile
{
    /// <summary>
    /// Cập nhật hồ sơ khách hàng (họ tên + số điện thoại). Email là định danh
    /// đăng nhập nên không cho sửa ở đây. CustomerId lấy từ Token.
    /// </summary>
    public class UpdateCustomerProfileCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long CustomerId { get; set; }

        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;
    }
}
