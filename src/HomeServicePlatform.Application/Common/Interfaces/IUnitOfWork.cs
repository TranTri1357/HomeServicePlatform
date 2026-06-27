using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Hàm chốt hạ lưu tất cả thay đổi của các Repository xuống DB cùng một lúc
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Hàm mở rộng nếu sau này bạn muốn tự quản lý Transaction bằng tay
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
