using System;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>Kết quả tính toán chính sách hủy/hoàn tiền cho một đơn.</summary>
    public readonly record struct RefundDecision(
        bool CanCancel,        // Trạng thái đơn có cho phép hủy không
        int RefundPercent,     // % hoàn cho khách trên số tiền đã thanh toán qua hệ thống
        decimal RefundAmount,  // Tiền hoàn cho khách (đã làm tròn về đồng)
        decimal PenaltyAmount, // Phí hủy giữ lại = totalPaid - RefundAmount (đền cho thợ)
        string Reason);        // Diễn giải chính sách để hiển thị cho người dùng

    /// <summary>
    /// Bộ tính chính sách hoàn tiền — hàm THUẦN (không đụng DB), tất định, dễ unit-test.
    /// Cân bằng lợi ích 3 bên: khách được hủy miễn phí trong vùng rộng; thợ được đền công
    /// khi khách hủy muộn; sàn cấu hình được mọi ngưỡng qua <see cref="RefundPolicyOptions"/>.
    /// </summary>
    public static class RefundPolicy
    {
        public static RefundDecision Calculate(
            BookingStatus status,
            DateTimeOffset? scheduledAt,
            DateTimeOffset now,
            RefundInitiator actor,
            decimal totalPaid,
            RefundPolicyOptions options)
        {
            // 1) Thợ / Admin / Hệ thống hủy: lỗi không thuộc khách -> hoàn 100%, không phí.
            if (actor != RefundInitiator.Customer)
            {
                if (status == BookingStatus.Completed || status == BookingStatus.Cancelled || status == BookingStatus.Refund)
                    return Deny("Đơn đã kết thúc nên không thể hủy.");

                return Build(100, totalPaid,
                    actor == RefundInitiator.Tasker
                        ? "Thợ hủy đơn — hoàn 100% cho khách."
                        : "Hệ thống/Admin hủy đơn — hoàn 100% cho khách.");
            }

            // 2) Khách hủy: xét theo trạng thái đơn + thời gian còn lại tới giờ hẹn.
            switch (status)
            {
                case BookingStatus.Pending:
                    return Build(100, totalPaid, "Thợ chưa nhận đơn — hoàn 100%.");

                case BookingStatus.Accepted:
                    var hoursLeft = scheduledAt.HasValue
                        ? (scheduledAt.Value - now).TotalHours
                        : double.MaxValue;
                    if (hoursLeft > options.FreeCancelHours)
                        return Build(100, totalPaid,
                            $"Hủy sớm (còn hơn {options.FreeCancelHours:0.#} giờ tới giờ hẹn) — hoàn 100%.");
                    return Build(options.LateAcceptedRefundPercent, totalPaid,
                        $"Hủy sát giờ hẹn (còn ≤ {options.FreeCancelHours:0.#} giờ) — hoàn {options.LateAcceptedRefundPercent}%, phần còn lại đền công thợ đã sắp lịch.");

                case BookingStatus.OnTheWay:
                    return Build(options.OnTheWayRefundPercent, totalPaid,
                        options.OnTheWayRefundPercent > 0
                            ? $"Thợ đang trên đường — hoàn {options.OnTheWayRefundPercent}%."
                            : "Thợ đang trên đường tới — không hoàn, toàn bộ đền công di chuyển của thợ.");

                default:
                    // InProgress / Completed / Cancelled / Refund
                    return Deny("Đơn đang thực hiện hoặc đã kết thúc — không thể tự hủy. Nếu có vấn đề, vui lòng gửi khiếu nại.");
            }
        }

        private static RefundDecision Build(int percent, decimal totalPaid, string reason)
        {
            var refund = Math.Round(totalPaid * percent / 100m, 0, MidpointRounding.AwayFromZero);
            if (refund > totalPaid) refund = totalPaid;
            var penalty = totalPaid - refund;
            return new RefundDecision(true, percent, refund, penalty, reason);
        }

        private static RefundDecision Deny(string reason) =>
            new RefundDecision(false, 0, 0m, 0m, reason);
    }
}
