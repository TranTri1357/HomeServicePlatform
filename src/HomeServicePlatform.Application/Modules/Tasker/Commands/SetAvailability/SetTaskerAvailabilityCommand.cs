using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.SetAvailability
{
    /// <summary>
    /// Bật/tắt trạng thái nhận việc của thợ (Status: 1 = nhận việc, 3 = tạm nghỉ).
    /// TaskerId (= UserId) lấy từ Token. Trả về trạng thái mới.
    /// </summary>
    public class SetTaskerAvailabilityCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long TaskerId { get; set; }

        public bool IsAvailable { get; set; }
    }
}
