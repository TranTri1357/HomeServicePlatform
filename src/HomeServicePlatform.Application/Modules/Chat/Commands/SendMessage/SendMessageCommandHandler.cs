using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Chat.Dtos;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ApiResponse<MessageDto>>
    {
        private readonly IApplicationDbContext _context;

        public SendMessageCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn #{request.BookingId}.");

            bool isCustomer = booking.CustomerId == request.SenderId;
            bool isTasker = await _context.BookingItems
                .AnyAsync(bi => bi.BookingId == request.BookingId && bi.TaskerId == request.SenderId, ct);

            if (!isCustomer && !isTasker)
                throw new ForbiddenException("Bạn không có quyền nhắn tin trong đơn này.");

            var message = new Message
            {
                BookingId = request.BookingId,
                SenderId = request.SenderId,
                Content = request.Content.Trim(),
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(ct);

            var senderName = await _context.Users
                .Where(u => u.UserId == request.SenderId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct) ?? "Người dùng";

            var dto = new MessageDto(
                message.MessageId,
                message.BookingId,
                message.SenderId,
                senderName,
                message.Content,
                message.IsRead,
                message.CreatedAt);

            return ApiResponse<MessageDto>.Success(dto, "Đã gửi tin nhắn.");
        }
    }
}
