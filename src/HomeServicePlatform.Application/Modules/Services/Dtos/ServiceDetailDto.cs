using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Dtos
{
    public class ServiceDetailDto
    {
        public long ServiceId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }

        public int TotalBookings { get; set; }

        public decimal StartingPrice { get; set; }
        public string? ImageUrl { get; set; }

        public List<ServiceTaskerSuggestionDto> SuggestedTaskers { get; set; } = new();
    }

    public class ServiceTaskerSuggestionDto
    {
        public long TaskerId { get; set; }
        public string FullName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public int ExperienceYears { get; set; }
        public decimal RatingAvg { get; set; }
        public decimal CurrentPrice { get; set; }

        // 📍 Khu vực hoạt động của thợ, lấy từ địa chỉ mặc định trong bảng Addresses.
        // CHỈ trả mã tỉnh/quận — KHÔNG trả AddressLine: khách chỉ cần biết thợ ở khu vực
        // nào để ước lượng xa gần, còn số nhà của thợ là thông tin riêng tư.
        // Null khi thợ chưa khai địa chỉ (hồ sơ cũ) — FE phải chịu được trường hợp này.
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }

        // Khoảng cách đường chim bay từ địa chỉ đặt của khách tới nơi ở của thợ.
        // Null khi thiếu tọa độ một trong hai đầu (khách chưa chọn địa chỉ đã lưu,
        // hoặc địa chỉ thợ chưa được geocode).
        public double? DistanceKm { get; set; }
    }
}
