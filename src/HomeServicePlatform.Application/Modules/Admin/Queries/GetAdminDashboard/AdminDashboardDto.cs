using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Admin.Queries.GetAdminDashboard
{
    /// <remarks>
    /// ⚠️ Phân biệt hai loại "doanh thu" — trước đây bị gộp làm một và gọi sai tên:
    ///   • <b>TodayRevenue / TotalRevenue</b> = GMV (tổng giá trị giao dịch) — tổng tiền khách
    ///     trả cho các đơn đã hoàn thành. Đây KHÔNG phải tiền của sàn, phần lớn thuộc về thợ.
    ///   • <b>TodayPlatformRevenue / TotalPlatformRevenue</b> = doanh thu THẬT của sàn — hoa hồng
    ///     và phí hủy đã ghi vào ví doanh thu. Đọc thẳng từ sổ ví nên luôn khớp số dư thực tế.
    /// </remarks>
    public record AdminDashboardDto(
        decimal TodayRevenue,
        int TodayBookings,
        int TotalCustomers,
        int TotalTaskers,
        int ActiveTaskers,
        int PendingTaskers,
        int TotalBookings,
        decimal TotalRevenue,
        decimal TodayPlatformRevenue,
        decimal TotalPlatformRevenue,
        int OpenDisputes,
        List<DailyRevenuePoint> WeeklyRevenue,     // 7 ngày, cũ -> mới
        List<StatusCountDto> BookingsByStatus,     // số đơn theo từng trạng thái
        List<RecentBookingDto> RecentBookings      // 5 đơn gần nhất
    );

    public record DailyRevenuePoint(DateTime Date, decimal Amount);

    public record StatusCountDto(short Status, int Count);

    public record RecentBookingDto(
        long BookingId,
        string CustomerName,
        decimal FinalAmount,
        short Status,
        DateTimeOffset CreatedAt
    );
}
