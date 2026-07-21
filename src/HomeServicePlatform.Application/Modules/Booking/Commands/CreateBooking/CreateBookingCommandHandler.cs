using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<CreateBookingResponse>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;
        private readonly BufferPolicyOptions _buffer;
        private readonly BookingPolicyOptions _bookingPolicy;
        private readonly GeometryFactory _geometryFactory;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork,
            IApplicationDbContext context, IOptions<BufferPolicyOptions> buffer,
            IOptions<BookingPolicyOptions> bookingPolicy)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _context = context;
            _buffer = buffer.Value;
            _bookingPolicy = bookingPolicy.Value;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            if (request.BookingItems == null || !request.BookingItems.Any())
            {
                throw new BadRequestException("Đơn đặt lịch bắt buộc phải có ít nhất một hạng mục dịch vụ.");
            }

            var nowUtc = DateTimeOffset.UtcNow;


            if (request.BookingItems.Any(i => !i.TaskerId.HasValue))
            {
                throw new BadRequestException("Mỗi hạng mục dịch vụ phải chọn một thợ cụ thể để xác định giá.");
            }

            var taskerIds = request.BookingItems.Select(i => i.TaskerId!.Value).Distinct().ToList();
            var serviceIds = request.BookingItems.Select(i => i.ServiceId).Distinct().ToList();
            var priceRows = await _context.TaskerServicePrices
                .Where(p => taskerIds.Contains(p.TaskerId) && serviceIds.Contains(p.ServiceId))
                .ActiveAt(nowUtc)
                .ToListAsync(cancellationToken);

            var booking = new Domain.Modules.Bookings.Entities.Booking
            {
                CustomerId = request.CustomerId,
                Note = request.Note,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                RowVersion = 1
            };

            booking.InitializeBooking(request.CustomerId);

            decimal totalSubtotalAmount = 0m;

            foreach (var item in request.BookingItems)
            {
                DateTime startAtUtc = item.StartAt.ToUniversalTime();
                DateTime endAtUtc = item.EndAt.ToUniversalTime();
                int durationMinutes = (int)(endAtUtc - startAtUtc).TotalMinutes;

                if (durationMinutes <= 0)
                {
                    throw new BadRequestException($"Dịch vụ mã #{item.ServiceId} có thời gian kết thúc bắt buộc phải lớn hơn thời gian bắt đầu.");
                }

                var listedPrice = priceRows
                    .Where(p => p.TaskerId == item.TaskerId!.Value && p.ServiceId == item.ServiceId)
                    .OrderByDescending(p => p.EffectiveFrom)
                    .Select(p => (decimal?)p.Price)
                    .FirstOrDefault();

                if (listedPrice is null)
                {
                    throw new BadRequestException($"Thợ chưa niêm yết giá cho dịch vụ #{item.ServiceId}. Vui lòng chọn thợ hoặc dịch vụ khác.");
                }

                decimal serverUnitPrice = listedPrice.Value;
                decimal itemTotalPrice = serverUnitPrice * item.Quantity;
                totalSubtotalAmount += itemTotalPrice;

                booking.BookingItems.Add(new BookingItem
                {
                    ServiceId = item.ServiceId,
                    TaskerId = item.TaskerId,
                    StartAt = startAtUtc,
                    EndAt = endAtUtc,
                    Quantity = item.Quantity,
                    DurationMinutes = durationMinutes,
                    UnitPrice = serverUnitPrice,
                    TotalPrice = itemTotalPrice,
                    Status = (short)BookingStatus.Pending,
                    RowVersion = 1
                });
            }

            decimal discount = 0m;
            booking.SubtotalAmount = totalSubtotalAmount;
            booking.DiscountAmount = discount;
            booking.FinalAmount = Math.Max(0m, totalSubtotalAmount - discount);

            Point? locationGeom = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                locationGeom = _geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
            }

            booking.BookingAddress = new BookingAddress
            {
                FullName = request.FullName,
                Phone = request.Phone,
                ProvinceCode = request.ProvinceCode,
                DistrictCode = request.DistrictCode,
                WardCode = request.WardCode,
                AddressLine = request.AddressLine,
                Geom = locationGeom
            };

            var destLat = request.Latitude;
            var destLng = request.Longitude;
            var lockTaskerIds = booking.BookingItems
                .Where(i => i.TaskerId.HasValue)
                .Select(i => i.TaskerId!.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var tid in lockTaskerIds)
                    await _context.AcquireTaskerScheduleLockAsync(tid, cancellationToken);

                foreach (var item in booking.BookingItems.Where(i => i.TaskerId.HasValue))
                {
                    if (_bookingPolicy.EnforceWorkingHours)
                        await WorkingHoursGuard.EnsureWithinWorkingHoursAsync(
                            _context, item.TaskerId!.Value, item.StartAt, item.EndAt, cancellationToken);

                    await TravelBufferGuard.EnsureTravelFeasibleAsync(
                        _context, _buffer, item.TaskerId!.Value, item.StartAt, item.EndAt, destLat, destLng, cancellationToken);
                }

                await _bookingRepository.SaveAggregateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException ex) when (IsExclusionViolation(ex))
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new BadRequestException(
                    "Rất tiếc, khung giờ bạn chọn của thợ vừa có người khác đặt trước. Vui lòng chọn khung giờ hoặc thợ khác.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }


            var responseData = new CreateBookingResponse(
                booking.BookingId,
                "Đặt lịch hệ thống thành công!",
                true,
                booking.FinalAmount
            );

            return ApiResponse<CreateBookingResponse>.Success(responseData, "Khởi tạo đơn đặt lịch thành công.");
        }

        private static bool IsExclusionViolation(Exception ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner) as string;
                if (sqlState == "23P01")
                    return true;
            }
            return false;
        }
    }
}
