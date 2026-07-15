using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Storage
{
    /// <summary>
    /// Cài đặt <see cref="IImageStorage"/> dùng Cloudinary. Tự tối ưu ảnh (giới hạn kích thước,
    /// tự chọn định dạng/chất lượng) và trả về URL https an toàn để lưu vào DB.
    /// </summary>
    public class CloudinaryImageStorage : IImageStorage
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageStorage(IOptions<CloudinaryOptions> options)
        {
            var o = options.Value;
            if (string.IsNullOrWhiteSpace(o.CloudName) || string.IsNullOrWhiteSpace(o.ApiKey) || string.IsNullOrWhiteSpace(o.ApiSecret))
                throw new InvalidOperationException(
                    "Chưa cấu hình Cloudinary. Hãy đặt Cloudinary:CloudName / ApiKey / ApiSecret qua User Secrets (local) hoặc Environment Variables (production).");

            _cloudinary = new Cloudinary(new Account(o.CloudName, o.ApiKey, o.ApiSecret)) { Api = { Secure = true } };
        }

        public async Task<string> UploadImageAsync(Stream content, string fileName, string folder, CancellationToken ct = default)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, content),
                Folder = string.IsNullOrWhiteSpace(folder) ? "misc" : folder,
                UniqueFilename = true,
                Overwrite = false,
                // Tối ưu: giới hạn cạnh dài 1000px (không phóng to ảnh nhỏ), tự chọn chất lượng + định dạng.
                Transformation = new Transformation().Width(1000).Crop("limit").Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error != null)
                throw new InvalidOperationException($"Tải ảnh lên Cloudinary thất bại: {result.Error.Message}");

            return result.SecureUrl?.ToString()
                   ?? throw new InvalidOperationException("Cloudinary không trả về URL ảnh.");
        }
    }
}
