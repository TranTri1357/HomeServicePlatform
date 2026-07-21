namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    public record CancellationPreviewDto(
        long BookingId,
        bool CanCancel,
        short Status,
        decimal TotalPaid,
        int RefundPercent,
        decimal RefundAmount,
        decimal PenaltyAmount,
        decimal DepositAtRisk,
        string Reason
    );
}
