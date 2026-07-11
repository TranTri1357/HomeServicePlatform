using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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
        public static string UserGroup(long userId) => $"user-{userId}";

        public override async Task OnConnectedAsync()
        {
            if (TryGetUserId(out var userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            await base.OnConnectedAsync();
        }

        public async Task StreamDriverLocation(string providerId, double latitude, double longitude)
        {
            await Clients.All.SendAsync("ReceiveDriverLocation", providerId, latitude, longitude);
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
