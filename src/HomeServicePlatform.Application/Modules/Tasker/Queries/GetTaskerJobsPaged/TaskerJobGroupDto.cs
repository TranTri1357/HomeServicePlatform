using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged
{
    /// <summary>Một hạng mục (dịch vụ) được giao cho thợ trong một đơn.</summary>
    public record TaskerJobItemDto(
        long BookingItemId,
        string ServiceName,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        decimal TotalPrice,
        short ItemStatus
    );

    /// <summary>
    /// Một đơn (Booking) gom các hạng mục được giao cho thợ này — gộp sẵn ở server
    /// để giao diện thợ không phải gộp lại và không phải tải toàn bộ danh sách.
    /// </summary>
    public record TaskerJobGroupDto(
        long BookingId,
        string CustomerName,
        string CustomerPhone,
        string FullAddress,
        short JobStatus,            // Trạng thái đơn tổng (nguồn chuẩn)
        DateTimeOffset StartAt,     // Giờ bắt đầu sớm nhất của thợ trong đơn
        decimal Total,              // Tổng tiền các hạng mục của thợ trong đơn
        List<TaskerJobItemDto> Items
    );
}
