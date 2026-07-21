namespace HomeServicePlatform.Domain.Modules.Operations.Enum
{
    public enum NotificationType : short
    {
        NewBooking = 1,
        BookingCancelledByCustomer = 2,
        NewReview = 3,
        EmergencyBooking = 4,
        ProfileApproved = 5,
        ProfileRejected = 6,
        BookingExpiredUnconfirmed = 7,

        BookingAccepted = 10,
        TaskerOnTheWay = 11,
        WorkStarted = 12,
        WorkCompleted = 13,
        BookingCancelledByTasker = 14,
        RefundIssued = 15,

        DisputeResolved = 16,
        DisputeRejected = 17,
        BookingAutoCancelled = 18,

        BookingDeclinedByTasker = 19
    }
}
