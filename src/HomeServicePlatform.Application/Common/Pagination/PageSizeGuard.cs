namespace HomeServicePlatform.Application.Common.Pagination
{
    /// <summary>
    /// Chuẩn hoá tham số phân trang do client gửi lên — chống DoS/payload khổng lồ khi
    /// client gửi pageSize cực lớn (vd 1_000_000). Dùng chung cho mọi query có phân trang.
    /// </summary>
    public static class PageSizeGuard
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        /// <summary>Ép pageSize về [1, max]; nếu &lt; 1 thì trả về mặc định.</summary>
        public static int Clamp(int pageSize, int max = MaxPageSize, int @default = DefaultPageSize)
        {
            if (pageSize < 1) return @default;
            return pageSize > max ? max : pageSize;
        }

        /// <summary>Ép pageIndex/pageNumber tối thiểu là 1.</summary>
        public static int ClampIndex(int pageIndex) => pageIndex < 1 ? 1 : pageIndex;
    }
}
