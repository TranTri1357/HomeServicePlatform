namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    public enum WalletTransactionType : short
    {
        TopUp = 1,
        Payment = 2,
        Refund = 3,
        Earning = 4,
        Withdraw = 5,
        Adjustment = 6,

        EscrowIn = 7,
        EscrowOut = 8,
        Commission = 9
    }
}
