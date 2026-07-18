namespace HomeServicePlatform.Domain.Modules.Payments.Constants
{
    /// <summary>
    /// Định danh hai "tài khoản hệ thống" giữ dòng tiền của sàn. Vì bảng ví có khóa ngoại
    /// 1-1 sang users nên mỗi ví hệ thống phải gắn với một user hệ thống (được seed sẵn,
    /// khóa đăng nhập, không gán vai trò nào).
    ///
    /// ID đặt ở dải rất cao để không bao giờ đụng chuỗi identity của người dùng thật.
    /// </summary>
    public static class SystemAccounts
    {
        /// <summary>Ví KÝ QUỸ: giữ hộ tiền đã thu của các đơn chưa tất toán (chưa hoàn thành / chưa hủy).</summary>
        public const long EscrowUserId = 9_000_000_001L;

        /// <summary>Ví DOANH THU: hoa hồng đã chốt + phí hủy sàn giữ lại.</summary>
        public const long RevenueUserId = 9_000_000_002L;

        /// <summary>Khóa chính ví tương ứng (seed cứng để tránh phụ thuộc thứ tự identity).</summary>
        public const long EscrowWalletId = 9_000_000_001L;
        public const long RevenueWalletId = 9_000_000_002L;

        /// <summary>Tài khoản hệ thống không phải người dùng — chặn mọi thao tác ví do client khởi xướng.</summary>
        public static bool IsSystemAccount(long userId)
            => userId == EscrowUserId || userId == RevenueUserId;
    }
}
