namespace HomeServicePlatform.Application.Common.Options
{
    public class BufferPolicyOptions
    {
        public const string SectionName = "BufferPolicy";

        public double AverageSpeedKmh { get; set; } = 25;

        public double DetourFactor { get; set; } = 1.3;

        public int MinBufferMinutes { get; set; } = 15;

        public int MaxBufferMinutes { get; set; } = 90;

        public int HardFloorMinutes { get; set; } = 20;
    }
}
