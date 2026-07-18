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

            // 3. 🛡️ MỘT ĐƠN = MỘT THỢ.
            //    Về cấu trúc, TaskerId nằm ở TỪNG hạng mục nên schema cho phép mỗi hạng mục một
            //    thợ khác nhau. Nhưng phần TIỀN chưa hỗ trợ điều đó: khi tất toán,
            //    CompleteWorkCommandHandler lấy toàn bộ số ký quỹ của đơn giao cho thợ bấm hoàn
            //    thành TRƯỚC, còn thợ thứ hai bị chốt idempotent chặn lại và nhận 0đ.
            //    Frontend hiện luôn gửi cùng một thợ, nhưng ai gọi thẳng API thì không bị chặn —
            //    nên biến giả định ngầm đó thành ràng buộc được kiểm tra tại đây.
            //    ⚠️ Muốn mở nhiều thợ trên một đơn: bỏ luật này SAU KHI đã chia ký quỹ theo tỷ
            //    trọng TotalPrice của từng thợ và chốt idempotent theo cặp (bookingId, taskerId).
            RuleFor(x => x.BookingItems)
                .Must(items => items.Select(i => i.TaskerId).Distinct().Count() == 1)
                .WithMessage("Tất cả hạng mục trong cùng một đơn phải do cùng một thợ đảm nhận.")
                .When(x => x.BookingItems != null && x.BookingItems.Count > 0);
        }
    }
    public class BookingItemDtoValidator : AbstractValidator<BookingItemDto>
    {
        public BookingItemDtoValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ con không hợp lệ.");

            // 🛡️ Bắt buộc chọn thợ cụ thể để server tra được ĐƠN GIÁ NIÊM YẾT thật.
            RuleFor(x => x.TaskerId)
                .NotNull().WithMessage("Vui lòng chọn thợ cho từng hạng mục dịch vụ.")
                .Must(id => id is null || id > 0).WithMessage("Mã thợ không hợp lệ.");

            // ⚠️ KHÔNG validate UnitPrice: server tự tra giá niêm yết và bỏ qua giá client gửi
            //    (chống giả mạo giá). Giữ field trong DTO chỉ để tương thích payload cũ.

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