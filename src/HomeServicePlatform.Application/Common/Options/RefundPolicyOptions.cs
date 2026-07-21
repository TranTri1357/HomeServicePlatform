namespace HomeServicePlatform.Application.Common.Options
{
    public class RefundPolicyOptions
    {
        public const string SectionName = "RefundPolicy";

        public double FreeCancelHours { get; set; } = 2;

        public int LateAcceptedRefundPercent { get; set; } = 50;

        public int OnTheWayRefundPercent { get; set; } = 0;

        public int TaskerCancelSuspendThreshold { get; set; } = 3;

        public int TaskerConfirmDeadlineMinutes { get; set; } = 30;
    }
}
