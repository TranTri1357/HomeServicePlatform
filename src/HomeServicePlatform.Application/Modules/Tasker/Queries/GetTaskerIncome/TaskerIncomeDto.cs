using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    public class TaskerIncomeDto
    {
        /// <summary>Số dư ví hiện tại của thợ.</summary>
        public decimal Balance { get; set; }
        /// <summary>Tổng thực nhận đã ghi có từ trước tới nay.</summary>
        public decimal TotalEarned { get; set; }
        /// <summary>Tổng số dòng thu nhập (để phân trang phía FE).</summary>
        public int TotalCount { get; set; }
        public List<IncomeEntryDto> Entries { get; set; } = new();
    }

    /// <summary>Một lần ghi có thu nhập ứng với một đơn đã hoàn thành.</summary>
    public record IncomeEntryDto(
        long TransactionId,
        long BookingId,
        string ServiceSummary, // tên dịch vụ đại diện của đơn
        decimal Gross,         // giá gộp (trước hoa hồng)
        decimal Commission,    // hoa hồng đã trừ
        decimal Net,           // thực nhận (ghi có ví)
        decimal BalanceAfter,
        DateTimeOffset CreatedAt
    );
}
