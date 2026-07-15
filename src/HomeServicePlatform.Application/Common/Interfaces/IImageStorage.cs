using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    /// <summary>
    /// Trừu tượng lưu trữ ảnh (upload lên dịch vụ bên thứ 3, hiện là Cloudinary). Tầng Application chỉ
    /// biết interface này; cài đặt cụ thể nằm ở Infrastructure nên có thể thay nhà cung cấp mà không đụng
    /// nghiệp vụ.
    /// </summary>
    public interface IImageStorage
    {
        /// <summary>
        /// Tải một ảnh lên và trả về URL công khai (https). <paramref name="folder"/> gom ảnh theo nhóm
        /// (ví dụ "categories", "services").
        /// </summary>
        Task<string> UploadImageAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    }
}
