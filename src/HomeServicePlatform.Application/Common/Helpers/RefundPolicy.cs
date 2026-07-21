using System;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public readonly record struct RefundDecision(
        bool CanCancel,
        int RefundPercent,
        decimal RefundAmount,
        decimal PenaltyAmount,
        decimal DepositAtRisk,
        string Reason);

    public static class RefundPolicy
    {
        public static decimal DepositOf(decimal orderTotal, int depositPercent)
        {
            var pct = Math.Clamp(depositPercent, 0, 100);
            if (orderTotal <= 0m) return 0m;
            return Math.Round(orderTotal * pct / 100m, 0, MidpointRounding.AwayFromZero);
        }

        public static RefundDecision Calculate(
            BookingStatus status,
            DateTimeOffset? scheduledAt,
            DateTimeOffset now,
            RefundInitiator actor,
            decimal totalPaid,
            decimal orderTotal,
            int depositPercent,
            RefundPolicyOptions options)
        {
            var deposit = Math.Min(DepositOf(orderTotal, depositPercent), totalPaid);

            if (actor != RefundInitiator.Customer)
            {
                if (status == BookingStatus.Completed || status == BookingStatus.Cancelled || status == BookingStatus.Refund)
                    return Deny("Đơn đã kết thúc nên không thể hủy.");

                return Build(100, totalPaid, deposit,
                    actor == RefundInitiator.Tasker
                        ? "Thợ hủy đơn — hoàn 100% cho khách."
                        : "Hệ thống/Admin hủy đơn — hoàn 100% cho khách.");
            }

            switch (status)
            {
                case BookingStatus.Pending:
                    return Build(100, totalPaid, deposit, "Thợ chưa nhận đơn — hoàn 100%.");

                case BookingStatus.Accepted:
                    var hoursLeft = scheduledAt.HasValue
                        ? (scheduledAt.Value - now).TotalHours
                        : double.MaxValue;
                    if (hoursLeft > options.FreeCancelHours)
                        return Build(100, totalPaid, deposit,
                            $"Hủy sớm (còn hơn {options.FreeCancelHours:0.#} giờ tới giờ hẹn) — hoàn 100%.");
                    return Build(options.LateAcceptedRefundPercent, totalPaid, deposit,
                        $"Hủy sát giờ hẹn (còn ≤ {options.FreeCancelHours:0.#} giờ) — khấu trừ "
                        + $"{100 - options.LateAcceptedRefundPercent}% tiền cọc để đền công thợ đã sắp lịch, "
                        + "phần đã thanh toán vượt mức cọc được hoàn đủ.");

                case BookingStatus.OnTheWay:
                    return Build(options.OnTheWayRefundPercent, totalPaid, deposit,
                        options.OnTheWayRefundPercent > 0
                            ? $"Thợ đang trên đường — khấu trừ {100 - options.OnTheWayRefundPercent}% tiền cọc, "
                              + "phần đã thanh toán vượt mức cọc được hoàn đủ."
                            : "Thợ đang trên đường tới — khấu trừ toàn bộ tiền cọc để đền công di chuyển, "
                              + "phần đã thanh toán vượt mức cọc được hoàn đủ.");

                default:
                    return Deny("Đơn đang thực hiện hoặc đã kết thúc — không thể tự hủy. Nếu có vấn đề, vui lòng gửi khiếu nại.");
            }
        }

        private static RefundDecision Build(int percent, decimal totalPaid, decimal deposit, string reason)
        {
            var aboveDeposit = totalPaid - deposit;
            var refundOfDeposit = Math.Round(deposit * percent / 100m, 0, MidpointRounding.AwayFromZero);

            var refund = aboveDeposit + refundOfDeposit;
            if (refund > totalPaid) refund = totalPaid;
            if (refund < 0m) refund = 0m;

            var penalty = totalPaid - refund;

            return new RefundDecision(true, percent, refund, penalty, deposit, reason);
        }

        private static RefundDecision Deny(string reason) =>
            new RefundDecision(false, 0, 0m, 0m, 0m, reason);
    }
}
