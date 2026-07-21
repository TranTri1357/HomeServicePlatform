namespace HomeServicePlatform.Application.Common.Pagination
{
    public static class PageSizeGuard
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public static int Clamp(int pageSize, int max = MaxPageSize, int @default = DefaultPageSize)
        {
            if (pageSize < 1) return @default;
            return pageSize > max ? max : pageSize;
        }

        public static int ClampIndex(int pageIndex) => pageIndex < 1 ? 1 : pageIndex;
    }
}
