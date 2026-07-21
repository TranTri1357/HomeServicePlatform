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
        decimal TodayPlatformRevenue,
        decimal TotalPlatformRevenue,
        int OpenDisputes,
        List<DailyRevenuePoint> WeeklyRevenue,
        List<StatusCountDto> BookingsByStatus,
        List<RecentBookingDto> RecentBookings
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
