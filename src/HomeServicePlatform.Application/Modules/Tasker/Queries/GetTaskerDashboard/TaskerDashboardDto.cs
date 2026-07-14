using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    public record TaskerDashboardDto(
        string FullName,
        bool IsAvailable,          // Status == 1 (đang nhận việc)
        decimal RatingAvg,
        int TotalReviews,
        decimal TodayEarnings,     // Thực nhận hôm nay (sau hoa hồng), giờ VN
        int TodayJobsCount,        // Số việc có lịch hôm nay (chưa bị hủy)
        decimal MonthEarnings,     // Thực nhận trong tháng này (sau hoa hồng)
        decimal MonthGrossEarnings,// Doanh thu gộp tháng này (trước hoa hồng)
        decimal MonthCommission,   // Tổng hoa hồng đã trừ trong tháng
        List<DailyRevenueDto> WeeklyRevenue, // 7 ngày gần nhất (thực nhận), cũ -> mới
        int TotalJobsCount,        // Tổng số ĐƠN đã chốt của thợ (nhãn "X việc")
        List<TaskerTodayJobDto> TodayJobs   // Xem nhanh các việc có lịch hôm nay
    );

    public record DailyRevenueDto(DateTime Date, decimal Amount);

    /// <summary>Một việc (hạng mục) có lịch hôm nay — hiển thị nhanh ở trang chủ thợ.</summary>
    public record TaskerTodayJobDto(
        long BookingItemId,
        long BookingId,
        string ServiceName,
        string CustomerName,
        DateTimeOffset StartAt,
        decimal TotalPrice,
        short JobStatus,
        string FullAddress
    );
}
