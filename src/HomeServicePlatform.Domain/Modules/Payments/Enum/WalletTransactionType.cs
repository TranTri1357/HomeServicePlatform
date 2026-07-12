namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    /// <summary>Loại giao dịch ví (khớp cột smallint "type" của bảng wallet_transactions).</summary>
    public enum WalletTransactionType : short
    {
        TopUp = 1,      // Nạp tiền vào ví (ghi có)
        Payment = 2,    // Thanh toán bằng ví (ghi nợ)
        Refund = 3,     // Hoàn tiền vào ví (ghi có)
        Earning = 4,    // Thu nhập thợ sau khi trừ hoa hồng (ghi có)
        Withdraw = 5,   // Thợ rút tiền khỏi ví (ghi nợ)
        Adjustment = 6  // Điều chỉnh số dư thủ công (ghi có/nợ, demo coi là ghi có)
    }
}
