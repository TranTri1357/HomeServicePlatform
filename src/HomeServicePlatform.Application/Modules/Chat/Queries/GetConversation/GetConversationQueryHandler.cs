using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Chat.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Chat.Queries.GetConversation
{
    public class GetConversationQueryHandler
        : IRequestHandler<GetConversationQuery, ApiResponse<List<MessageDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetConversationQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<MessageDto>>> Handle(GetConversationQuery request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn #{request.BookingId}.");

            bool isCustomer = booking.CustomerId == request.UserId;
            bool isTasker = await _context.BookingItems
                .AnyAsync(bi => bi.BookingId == request.BookingId && bi.TaskerId == request.UserId, ct);

            if (!isCustomer && !isTasker)
                throw new ForbiddenException("Bạn không có quyền xem hội thoại của đơn này.");

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(m => m.BookingId == request.BookingId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto(
                    m.MessageId,
                    m.BookingId,
                    m.SenderId,
                    m.Sender.FullName,
                    m.Content,
                    m.IsRead,
                    m.CreatedAt))
                .ToListAsync(ct);

            return ApiResponse<List<MessageDto>>.Success(messages, "Lấy hội thoại thành công.");
        }
    }
}
