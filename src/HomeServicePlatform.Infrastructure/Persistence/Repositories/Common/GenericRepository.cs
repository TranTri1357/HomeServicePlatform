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

        protected readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy ra 1 bản ghi theo khóa chính ID
        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        // Lấy ra toàn bộ danh sách bản ghi trong bảng
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        // Tìm kiếm và lọc dữ liệu bằng câu lệnh Lambda (Linq) truyền từ Application xuống
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).ToListAsync();
        }

        // Thêm mới một bản ghi (Chỉ mới lưu vào bộ nhớ tạm của EF Core)
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        // Đánh dấu bản ghi là đã thay đổi để EF Core cập nhật khi SaveChanges
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        // Đánh dấu bản ghi là sẽ bị xóa khỏi Database
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
