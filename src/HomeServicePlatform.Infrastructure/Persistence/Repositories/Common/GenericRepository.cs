using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Repositories.Common
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // dbContext là file quản lý kết nối cơ sở dữ liệu chính của Entity Framework Core
        protected readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy ra 1 bản ghi theo khóa chính ID
        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        // 2. Lấy ra toàn bộ danh sách bản ghi trong bảng
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        // 3. Tìm kiếm và lọc dữ liệu bằng câu lệnh Lambda (Linq) truyền từ Application xuống
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).ToListAsync();
        }

        // 4. Thêm mới một bản ghi (Chỉ mới lưu vào bộ nhớ tạm của EF Core)
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        // 5. Đánh dấu bản ghi là đã thay đổi để EF Core cập nhật khi SaveChanges
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        // 6. Đánh dấu bản ghi là sẽ bị xóa khỏi Database
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
