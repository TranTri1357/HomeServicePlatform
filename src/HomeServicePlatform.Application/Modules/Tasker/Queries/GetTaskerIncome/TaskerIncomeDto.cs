using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    public class TaskerIncomeDto
    {
        /// <summary>Số dư ví hiện tại của thợ.</summary>
        public decimal Balance { get; set; }
        /// <summary>Tổng thực nhận (chỉ tính giao dịch Earning) từ trước tới nay.</summary>
        public decimal TotalEarned { get; set; }
        /// <summary>Tổng số dòng giao dịch (để phân trang phía FE).</summary>
        public int TotalCount { get; set; }
        public List<IncomeEntryDto> Entries { get; set; } = new();
    }

    /// <summary>Một dòng giao dịch ví của thợ (thu nhập, rút tiền, điều chỉnh...).</summary>
    public record IncomeEntryDto(
        long TransactionId,
        short Type,            // loại giao dịch (WalletTransactionType): 4=Earning, 5=Withdraw, 6=Adjustment...
        long BookingId,
        string ServiceSummary, // tên dịch vụ đại diện (đơn Earning) hoặc nhãn loại giao dịch
        decimal Gross,         // tổng giá đơn (trước hoa hồng) — chỉ có ý nghĩa với Earning
        decimal Commission,    // hoa hồng THẬT sàn đã khấu — chỉ có ý nghĩa với Earning
        decimal Net,           // số tiền của giao dịch ví (ghi có/nợ theo loại)
        decimal HeldAmount,    // tiền hệ thống đã giữ cho đơn (cọc/trả hết) — Earning
        decimal CashReceived,  // tiền mặt thợ thu trực tiếp = Gross − Held — Earning
        decimal BalanceAfter,
        DateTimeOffset CreatedAt
    );
}
