namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    public enum RefundStatus : short
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Rejected = 3
    }
}
