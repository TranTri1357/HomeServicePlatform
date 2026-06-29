using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System;
using System.Collections.Generic;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator()
        {
            // 1. Validate thông tin cơ bản của Đơn hàng cha (Booking Root)
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Mã khách hàng không hợp lệ.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên người nhận không được để trống.")
                .MaximumLength(100).WithMessage("Tên người nhận không được vượt quá 100 ký tự.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(03|05|07|08|09)\d{8}$").WithMessage("Số điện thoại không đúng định dạng di động Việt Nam (phải gồm 10 chữ số).");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống.");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Số tiền giảm giá không được âm.")
                .When(x => x.DiscountAmount.HasValue);

            // 2. 🔥 TÍCH HỢP VALIDATE MẢNG CON (BookingItems)
            RuleFor(x => x.BookingItems)
                .NotEmpty().WithMessage("Đơn đặt lịch bắt buộc phải có ít nhất một hạng mục dịch vụ.")
                .ForEach(item =>
                {
                    // Lặp qua từng phần tử trong mảng để áp dụng bộ luật chuyên biệt bên dưới
                    item.SetValidator(new BookingItemDtoValidator());
                });
        }
    }
    public class BookingItemDtoValidator : AbstractValidator<BookingItemDto>
    {
        public BookingItemDtoValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ con không hợp lệ.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Đơn giá của từng dịch vụ phải lớn hơn 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng đặt lịch tối thiểu của một hạng mục là 1.");

            // Đảm bảo thời gian hẹn phải là tương lai (Sử dụng DateTimeOffset hoặc DateTime tương đương hệ thống của bạn)
            RuleFor(x => x.StartAt)
                .GreaterThan(DateTime.UtcNow).WithMessage("Thời gian bắt đầu hẹn dịch vụ không được ở quá khứ.");

            // Ràng buộc thời gian kết thúc phải lớn hơn thời gian bắt đầu của CHÍNH hạng mục đó
            RuleFor(x => x.EndAt)
                .Must((item, endAt) => endAt > item.StartAt)
                .WithMessage("Thời gian kết thúc ca làm việc bắt buộc phải lớn hơn thời gian bắt đầu.");
        }
    }
}