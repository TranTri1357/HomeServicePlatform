using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;

        public CreateReviewCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<long>> Handle(CreateReviewCommand request, CancellationToken ct)
        {
            var bookingItem = await _context.BookingItems
                .Include(bi => bi.Booking)
                .Include(bi => bi.TaskerProfile)
                .FirstOrDefaultAsync(bi => bi.BookingItemId == request.BookingItemId, ct);


            if (bookingItem == null)
                throw new NotFoundException("Không tìm thấy hạng mục công việc này.");


            if (bookingItem.Booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền đánh giá đơn hàng của người khác.");


            if (bookingItem.Booking.Status != BookingStatus.Completed)
                throw new BadRequestException("Bạn chỉ có thể đánh giá khi công việc đã được hoàn thành.");


            if (!bookingItem.TaskerId.HasValue || bookingItem.TaskerProfile == null)
            {
                throw new InvalidOperationException("Dữ liệu đơn hàng bị lỗi: Đơn đã hoàn thành nhưng không tìm thấy thông tin thợ.");
            }


            bool hasReviewed = await _context.Reviews
                .AnyAsync(r => r.BookingItemId == request.BookingItemId && !r.IsDeleted, ct);
            if (hasReviewed)
                throw new BadRequestException("Bạn đã gửi đánh giá cho công việc này rồi.");

            var review = new Review
            {
                BookingItemId = request.BookingItemId,
                TaskerId = bookingItem.TaskerId.Value,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
            };

            _context.Reviews.Add(review);

            // Gọi hàm cập nhật điểm trung bình của Thợ (Rich Domain Model)
            bookingItem.TaskerProfile.UpdateRating(request.Rating);

            // 🔔 Thông báo cho thợ: có đánh giá mới từ khách.
            _context.Notifications.Add(Application.Common.Helpers.NotificationBuilder.Build(
                bookingItem.TaskerId.Value,
                Domain.Modules.Operations.Enum.NotificationType.NewReview,
                "Bạn có đánh giá mới",
                $"Khách vừa đánh giá {request.Rating}★ cho công việc của bạn."));

            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(review.ReviewId, "Cảm ơn bạn đã gửi đánh giá!");
        }
    }
}
