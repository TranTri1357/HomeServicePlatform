using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Đồng bộ vị trí hiện tại (CurrentGeom) của thợ theo địa chỉ hoạt động mặc định
    /// (Phương án 1). Được gọi từ các command địa chỉ (dùng chung Khách/Thợ) — có
    /// guard "chỉ áp dụng cho thợ" nên KHÔNG ảnh hưởng tài khoản khách.
    /// </summary>
    public static class TaskerLocationSync
    {
        /// <summary>
        /// Nếu <paramref name="userId"/> là thợ và <paramref name="defaultGeom"/> hợp lệ,
        /// gán CurrentGeom = tọa độ địa chỉ mặc định. Thay đổi được lưu cùng
        /// SaveChanges của command gọi nó (TaskerProfile được nạp ở chế độ tracking).
        /// </summary>
        public static async Task SyncFromDefaultAsync(
            IApplicationDbContext context, long userId, Point? defaultGeom, CancellationToken ct)
        {
            if (defaultGeom == null) return; // địa chỉ mặc định chưa có tọa độ → giữ nguyên

            var profile = await context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == userId && !t.IsDeleted, ct);
            if (profile == null) return; // không phải thợ → bỏ qua (khách không bị ảnh hưởng)

            profile.UpdateLocation(defaultGeom);
        }
    }
}
