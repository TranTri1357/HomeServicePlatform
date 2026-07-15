using System;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    /// <summary>
    /// Khung giờ trống của một thợ trong ngày (public, cho khách đặt lịch).
    /// <paramref name="Lat"/>/<paramref name="Lng"/> là TỌA ĐỘ ĐÍCH của đơn sắp đặt — có để
    /// hệ thống trừ thêm "thời gian đệm di chuyển" giữa các đơn cũ của thợ và địa điểm này,
    /// nên khách không chọn phải khung giờ mà thợ không kịp di chuyển tới. Bỏ trống ⇒ không trừ buffer.
    /// </summary>
    public record GetTaskerAvailabilityQuery(long TaskerId, DateOnly Date, double? Lat = null, double? Lng = null)
        : IRequest<ApiResponse<TaskerAvailabilityDto>>;
}
