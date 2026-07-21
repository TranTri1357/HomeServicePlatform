using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs
{
    public class GetTaskerJobsQueryHandler : IRequestHandler<GetTaskerJobsQuery, ApiResponse<List<TaskerJobDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerJobsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<TaskerJobDto>>> Handle(GetTaskerJobsQuery request, CancellationToken cancellationToken)
        {
            var sourceQuery = from item in _context.BookingItems
                              where item.TaskerId == request.TaskerId

                              join b in _context.Bookings on item.BookingId equals b.BookingId
                              join cust in _context.Users on b.CustomerId equals cust.UserId
                              join s in _context.Services on item.ServiceId equals s.ServiceId

                              join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId into addrGroup
                              from subAddr in addrGroup.DefaultIfEmpty()
                              select new { item, b, cust, s, subAddr };

            sourceQuery = sourceQuery.Where(q =>
                _context.Payments.Any(p => p.BookingId == q.b.BookingId
                                           && (p.Status == (short)PaymentStatus.Paid
                                               || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))));

            if (request.Status.HasValue)
            {
                sourceQuery = sourceQuery.Where(q => (short)q.b.Status == request.Status.Value);
            }

            sourceQuery = sourceQuery.OrderBy(q => q.item.StartAt);

            var result = await sourceQuery
                .Select(q => new TaskerJobDto(
                    q.item.BookingItemId,
                    q.b.BookingId,
                    q.s.Name,
                    q.subAddr != null ? q.subAddr.FullName : q.cust.FullName,
                    q.subAddr != null ? q.subAddr.Phone : q.cust.Phone,
                    q.item.StartAt,
                    q.item.EndAt,
                    q.subAddr != null ? q.subAddr.AddressLine : "Chưa cập nhật địa chỉ",
                    q.item.TotalPrice,
                    (short)q.b.Status
                ))
                .ToListAsync(cancellationToken);

            return ApiResponse<List<TaskerJobDto>>.Success(result, "Lấy danh sách công việc của thợ thành công.");
        }
    }
}
