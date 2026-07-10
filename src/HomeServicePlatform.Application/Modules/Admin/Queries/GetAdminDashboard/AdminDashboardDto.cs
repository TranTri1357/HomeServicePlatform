using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Admin.Queries.GetAdminDashboard
{
    public record AdminDashboardDto(
        decimal TodayRevenue,
        int TodayBookings,
        int TotalCustomers,
        int TotalTaskers,
        int ActiveTaskers,
        int PendingTaskers,
        int TotalBookings,
        decimal TotalRevenue,
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
