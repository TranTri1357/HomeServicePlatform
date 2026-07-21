using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.MarkConversationRead
{
    public class MarkConversationReadCommandHandler
        : IRequestHandler<MarkConversationReadCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public MarkConversationReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(MarkConversationReadCommand request, CancellationToken ct)
        {
            var unread = await _context.Messages
                .Where(m => m.BookingId == request.BookingId
                            && m.SenderId != request.UserId
                            && !m.IsRead)
                .ToListAsync(ct);

            if (unread.Count > 0)
            {
                foreach (var m in unread) m.IsRead = true;
                await _context.SaveChangesAsync(ct);
            }

            return ApiResponse<bool>.Success(true, "Đã đánh dấu đã đọc.");
        }
    }
}
