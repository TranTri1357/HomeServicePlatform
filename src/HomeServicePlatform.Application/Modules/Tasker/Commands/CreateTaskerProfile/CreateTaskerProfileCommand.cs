using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long UserId { get; set; }
        public string Bio { get; set; } = default!;
        public int ExperienceYears { get; set; }

        // Nullable có chủ đích: với double thường, client không gửi toạ độ sẽ bind thành 0.0 —
        // một giá trị hợp lệ (ngoài vịnh Guinea) — nên "chưa cung cấp vị trí" không phân biệt
        // được với "vị trí thật là 0,0" và validator không thể chặn.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>URL ảnh giấy tờ đã upload qua /api/tasker/uploads/image.</summary>
        public string VerificationImageUrl { get; set; } = default!;
    }
}
