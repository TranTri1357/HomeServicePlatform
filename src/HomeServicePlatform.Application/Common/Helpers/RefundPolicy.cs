using System;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>Kết quả tính toán chính sách hủy/hoàn tiền cho một đơn.</summary>
    public readonly record struct RefundDecision(
        bool CanCancel,        // Trạng thái đơn có cho phép hủy không
        int RefundPercent,     // % KHẤU TRỪ trên TIỀN CỌC (0 = không trừ đồng nào của cọc)
        decimal RefundAmount,  // Tiền hoàn cho khách (đã làm tròn về đồng)
        decimal PenaltyAmount, // Phí hủy giữ lại = totalPaid - RefundAmount (đền cho thợ)
        decimal DepositAtRisk, // Phần tiền CHỊU RỦI RO của đơn (= % cọc trên giá trị đơn)
        string Reason);        // Diễn giải chính sách để hiển thị cho người dùng

    /// <summary>
    /// Bộ tính chính sách hoàn tiền — hàm THUẦN (không đụng DB), tất định, dễ unit-test.
    /// Cân bằng lợi ích 3 bên: khách được hủy miễn phí trong vùng rộng; thợ được đền công
    /// khi khách hủy muộn; sàn cấu hình được mọi ngưỡng qua <see cref="RefundPolicyOptions"/>.
    ///
    /// 🧾 NGUYÊN TẮC CỐT LÕI — phí hủy KHÔNG phụ thuộc phương thức thanh toán:
    /// Chỉ phần TIỀN CỌC (mặc định 30% giá trị đơn) là khoản chịu rủi ro. Phần khách đã trả
    /// vượt quá mức cọc luôn được hoàn 100%.
    ///
    ///     cọc      = min(DepositPercent% × giá trị đơn, đã trả)
    ///     hoàn     = (đã trả − cọc) + cọc × phần trăm hoàn
    ///     phí hủy  = đã trả − hoàn   ( = cọc × phần trăm khấu trừ )
    ///
    /// Nhờ vậy khách "trả hết" và khách "đặt cọc" mất ĐÚNG BẰNG NHAU khi hủy cùng thời điểm.
    /// Trước đây công thức nhân thẳng phần trăm vào tổng đã trả — chỉ đúng cho khách đặt cọc
    /// (vì khi đó đã trả == cọc), còn khách trả hết bị phạt gấp nhiều lần cho cùng hành vi.
    /// </summary>
    public static class RefundPolicy
    {
        /// <summary>
        /// Tiền cọc của một đơn. Dùng CHUNG với <c>ProcessCheckout</c> để số tiền thu khi đặt
        /// cọc và phần chịu rủi ro khi hủy luôn là một.
        /// </summary>
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
            // Phần chịu rủi ro: không bao giờ vượt quá số khách đã thực trả (đơn tiền mặt trả 0
            // hoặc khách mới trả một phần thì kẹp lại, nếu không "phần vượt cọc" sẽ ra số ÂM).
            var deposit = Math.Min(DepositOf(orderTotal, depositPercent), totalPaid);

            // 1) Thợ / Admin / Hệ thống hủy: lỗi không thuộc khách -> hoàn 100%, không phí.
            if (actor != RefundInitiator.Customer)
            {
                if (status == BookingStatus.Completed || status == BookingStatus.Cancelled || status == BookingStatus.Refund)
                    return Deny("Đơn đã kết thúc nên không thể hủy.");

                return Build(100, totalPaid, deposit,
                    actor == RefundInitiator.Tasker
                        ? "Thợ hủy đơn — hoàn 100% cho khách."
                        : "Hệ thống/Admin hủy đơn — hoàn 100% cho khách.");
            }

            // 2) Khách hủy: xét theo trạng thái đơn + thời gian còn lại tới giờ hẹn.
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
                    // InProgress / Completed / Cancelled / Refund
                    return Deny("Đơn đang thực hiện hoặc đã kết thúc — không thể tự hủy. Nếu có vấn đề, vui lòng gửi khiếu nại.");
            }
        }

        /// <param name="percent">% được hoàn TRÊN TIỀN CỌC (100 = giữ nguyên cọc cho khách).</param>
        private static RefundDecision Build(int percent, decimal totalPaid, decimal deposit, string reason)
        {
            // Phần trả vượt mức cọc: hoàn vô điều kiện, không chịu bất kỳ khấu trừ nào.
            var aboveDeposit = totalPaid - deposit;
            var refundOfDeposit = Math.Round(deposit * percent / 100m, 0, MidpointRounding.AwayFromZero);

            var refund = aboveDeposit + refundOfDeposit;
            if (refund > totalPaid) refund = totalPaid;
            if (refund < 0m) refund = 0m;

            // ⚠️ Phí hủy tính bằng HIỆU chứ không tính riêng: đảm bảo tuyệt đối
            // refund + penalty == totalPaid kể cả khi làm tròn lệch 1đ. RefundExecutor dựa vào
            // bất biến này (platformFee = totalPaid − refund − penalty), lệch là tiền lẻ kẹt lại
            // vĩnh viễn trong ví doanh thu sau mỗi lần hủy.
            var penalty = totalPaid - refund;

            return new RefundDecision(true, percent, refund, penalty, deposit, reason);
        }

        private static RefundDecision Deny(string reason) =>
            new RefundDecision(false, 0, 0m, 0m, 0m, reason);
    }
}
