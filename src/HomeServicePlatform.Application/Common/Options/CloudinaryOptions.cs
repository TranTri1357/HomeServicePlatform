namespace HomeServicePlatform.Application.Common.Options
{
    /// <summary>
    /// Khóa cấu hình Cloudinary (đọc từ section "Cloudinary"). Giá trị thật KHÔNG để trong
    /// appsettings.json mà nạp từ User Secrets (local) hoặc Environment Variables (production),
    /// đúng yêu cầu tách secret khỏi mã nguồn.
    /// </summary>
    public class CloudinaryOptions
    {
        public const string SectionName = "Cloudinary";

        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }
}
