using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    public record TaskerDashboardDto(
        string FullName,
        bool IsAvailable,          // Status == 1 (đang nhận việc)
        decimal RatingAvg,
        int TotalReviews,
        decimal TodayEarnings,     // Tổng tiền các việc đã hoàn thành hôm nay (giờ VN)
        int TodayJobsCount,        // Số việc có lịch hôm nay (chưa bị hủy)
        decimal MonthEarnings,     // Tổng tiền hoàn thành trong tháng này
        List<DailyRevenueDto> WeeklyRevenue // 7 ngày gần nhất, cũ -> mới
    );

    public record DailyRevenueDto(DateTime Date, decimal Amount);
}
