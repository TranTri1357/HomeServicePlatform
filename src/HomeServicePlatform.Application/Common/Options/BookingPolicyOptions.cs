namespace HomeServicePlatform.Application.Common.Options
{
    public class BookingPolicyOptions
    {
        public const string SectionName = "BookingPolicy";

        public bool EnforceWorkingHours { get; set; } = true;

        public int DepositPercent { get; set; } = 30;
    }
}
