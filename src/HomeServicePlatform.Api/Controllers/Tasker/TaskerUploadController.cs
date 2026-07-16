using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    /// <summary>
    /// Tải ảnh giấy tờ xác minh (CCCD/chứng chỉ) của thợ lên Cloudinary qua proxy backend.
    /// Tách khỏi UploadController của Admin: thợ chỉ ghi được vào thư mục "taskers", không
    /// đụng tới ảnh danh mục/dịch vụ.
    /// </summary>
    [ApiController]
    [Route("api/tasker/uploads")]
    [Authorize(Roles = "Tasker")]
    public class TaskerUploadController : ControllerBase
    {
        private const long MaxBytes = 3 * 1024 * 1024; // 3MB
        private const string Folder = "taskers";
        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private readonly IImageStorage _imageStorage;

        public TaskerUploadController(IImageStorage imageStorage)
        {
            _imageStorage = imageStorage;
        }

        public record TaskerUploadImageResponse(string Url);

        [HttpPost("image")]
        [RequestSizeLimit(MaxBytes)]
        [ProducesResponseType(typeof(ApiResponse<TaskerUploadImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // Không đặt [FromForm] lên IFormFile: nó tự bind từ multipart, còn Swashbuckle sẽ
        // ném lỗi sinh Swagger nếu có attribute này.
        public async Task<IActionResult> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failure("Chưa chọn tệp ảnh."));

            if (file.Length > MaxBytes)
                return BadRequest(ApiResponse<object>.Failure("Ảnh vượt quá 3MB."));

            if (!AllowedTypes.Contains(file.ContentType))
                return BadRequest(ApiResponse<object>.Failure("Chỉ chấp nhận ảnh JPG, PNG, WEBP hoặc GIF."));

            await using var stream = file.OpenReadStream();
            var url = await _imageStorage.UploadImageAsync(stream, file.FileName, Folder);

            return Ok(ApiResponse<TaskerUploadImageResponse>.Success(new TaskerUploadImageResponse(url), "Tải ảnh lên thành công."));
        }
    }
}
