using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // 1. Lấy ra 1 bản ghi theo ID (Phục vụ cho các hàm xem chi tiết)
        Task<T?> GetByIdAsync(long id);

        // 2. Lấy ra TẤT CẢ bản ghi trong bảng
        Task<IEnumerable<T>> GetAllAsync();

        // 3. Tìm kiếm/Lọc dữ liệu theo điều kiện (Ví dụ: Tìm Booking có Status == Pending)
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

        // 4. Thêm mới 1 bản ghi vào Database
        Task AddAsync(T entity);

        // 5. Cập nhật thông tin bản ghi
        void Update(T entity);

        // 6. Xóa bản ghi khỏi Database
        void Delete(T entity);
    }
}
