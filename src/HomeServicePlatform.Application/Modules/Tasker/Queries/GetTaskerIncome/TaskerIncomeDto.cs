using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    public class TaskerIncomeDto
    {
        public decimal Balance { get; set; }
        public decimal TotalEarned { get; set; }
        public int TotalCount { get; set; }
        public List<IncomeEntryDto> Entries { get; set; } = new();
    }

    public record IncomeEntryDto(
        long TransactionId,
        short Type,
        long BookingId,
        string ServiceSummary,
        decimal Gross,
        decimal Commission,
        decimal Net,
        decimal HeldAmount,
        decimal CashReceived,
        decimal BalanceAfter,
        DateTimeOffset CreatedAt
    );
}
