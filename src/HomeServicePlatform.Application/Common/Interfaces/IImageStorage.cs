using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface IImageStorage
    {
        Task<string> UploadImageAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    }
}
