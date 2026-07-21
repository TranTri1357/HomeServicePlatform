using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    public record TaskerDashboardDto(
        string FullName,
        bool IsAvailable,
        decimal RatingAvg,
        int TotalReviews,
        decimal TodayEarnings,
        int TodayJobsCount,
        decimal MonthEarnings,
        decimal MonthGrossEarnings,
        decimal MonthCommission,
        List<DailyRevenueDto> WeeklyRevenue,
        int TotalJobsCount,
        List<TaskerTodayJobDto> TodayJobs
    );

    public record DailyRevenueDto(DateTime Date, decimal Amount);

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
