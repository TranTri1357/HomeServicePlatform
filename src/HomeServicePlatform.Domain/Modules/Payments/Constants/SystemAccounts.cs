namespace HomeServicePlatform.Domain.Modules.Payments.Constants
{
    public static class SystemAccounts
    {
        public const long EscrowUserId = 9_000_000_001L;

        public const long RevenueUserId = 9_000_000_002L;

        public const long EscrowWalletId = 9_000_000_001L;
        public const long RevenueWalletId = 9_000_000_002L;

        public static bool IsSystemAccount(long userId)
            => userId == EscrowUserId || userId == RevenueUserId;
    }
}
