using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Api.Hubs
{
    /// <summary>
    /// Hub chat theo đơn: mỗi đơn là 1 group ("booking-{id}"). Client gọi
    /// JoinConversation để vào group và nhận sự kiện "ReceiveMessage" realtime.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IApplicationDbContext _context;

        public ChatHub(IApplicationDbContext context)
        {
            _context = context;
        }

        public static string GroupName(long bookingId) => $"booking-{bookingId}";

        public async Task JoinConversation(long bookingId)
        {
            if (!TryGetUserId(out var userId)) return;
            if (!await IsParticipant(bookingId, userId)) return; // chỉ khách/thợ của đơn mới được vào
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(bookingId));
        }

        public Task LeaveConversation(long bookingId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(bookingId));

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Context.User?.FindFirst("uid")?.Value;
            return long.TryParse(claim, out userId);
        }

        private async Task<bool> IsParticipant(long bookingId, long userId)
        {
            var booking = await _context.Bookings.AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return false;
            if (booking.CustomerId == userId) return true;

            return await _context.BookingItems.AsNoTracking()
                .AnyAsync(bi => bi.BookingId == bookingId && bi.TaskerId == userId);
        }
    }
}
