using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/uploads")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UploadController : ControllerBase
    {
        private const long MaxBytes = 3 * 1024 * 1024;
        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };
        private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "categories", "services"
        };

        private readonly IImageStorage _imageStorage;

        public UploadController(IImageStorage imageStorage)
        {
            _imageStorage = imageStorage;
        }

        public record UploadImageResponse(string Url);

        [HttpPost("image")]
        [RequestSizeLimit(MaxBytes)]
        [ProducesResponseType(typeof(ApiResponse<UploadImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadImage(IFormFile? file, [FromQuery] string? folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failure("Chưa chọn tệp ảnh."));

            if (file.Length > MaxBytes)
                return BadRequest(ApiResponse<object>.Failure("Ảnh vượt quá 3MB."));

            if (!AllowedTypes.Contains(file.ContentType))
                return BadRequest(ApiResponse<object>.Failure("Chỉ chấp nhận ảnh JPG, PNG, WEBP hoặc GIF."));

            var targetFolder = folder != null && AllowedFolders.Contains(folder) ? folder.ToLowerInvariant() : "misc";

            await using var stream = file.OpenReadStream();
            var url = await _imageStorage.UploadImageAsync(stream, file.FileName, targetFolder);

            return Ok(ApiResponse<UploadImageResponse>.Success(new UploadImageResponse(url), "Tải ảnh lên thành công."));
        }
    }
}
