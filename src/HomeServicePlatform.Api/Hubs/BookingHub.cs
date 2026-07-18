using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Api.Hubs
{
    /// <summary>
    /// Hub theo dõi đơn hàng. Mỗi người dùng đăng nhập tự vào group riêng
    /// ("user-{id}") khi kết nối để nhận cập nhật trạng thái đơn realtime, và
    /// vẫn phục vụ streaming vị trí thợ như trước.
    /// </summary>
    [Authorize]
    public class BookingHub : Hub
    {
        private readonly IApplicationDbContext _context;

        public BookingHub(IApplicationDbContext context)
        {
            _context = context;
        }

        public static string UserGroup(long userId) => $"user-{userId}";

        public override async Task OnConnectedAsync()
        {
            if (TryGetUserId(out var userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Thợ đẩy vị trí GPS khi đang tới / đang làm. 🔒 Danh tính thợ lấy TỪ TOKEN (không nhận
        /// providerId từ client → chống giả mạo), và vị trí CHỈ gửi cho khách của đơn thợ đang
        /// phục vụ (OnTheWay/InProgress) thay vì phát cho tất cả.
        /// </summary>
        public async Task StreamDriverLocation(double latitude, double longitude)
        {
            if (!TryGetUserId(out var taskerId)) return;

            var customerIds = await _context.Bookings.AsNoTracking()
                .Where(b => (b.Status == BookingStatus.OnTheWay || b.Status == BookingStatus.InProgress)
                            && b.BookingItems.Any(i => i.TaskerId == taskerId))
                .Select(b => b.CustomerId)
                .Distinct()
                .ToListAsync();

            foreach (var customerId in customerIds)
                await Clients.Group(UserGroup(customerId))
                    .SendAsync("ReceiveDriverLocation", taskerId, latitude, longitude);
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Context.User?.FindFirst("uid")?.Value;
            return long.TryParse(claim, out userId);
        }
    }
}
