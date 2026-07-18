using FluentValidation;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");

            // Endpoint này chỉ Admin gọi được, nhưng vẫn chặn số tiền vô lý
            // để tránh sai sót nhập liệu làm hỏng đối soát.
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền thanh toán phải lớn hơn 0.")
                .LessThanOrEqualTo(500_000_000).WithMessage("Số tiền thanh toán vượt quá giới hạn cho phép.");

            // Method lưu dưới dạng short — kiểm tra phải là giá trị có định nghĩa
            // trong PaymentMethod (Wallet=1, Cash=2, Momo=3, ZaloPay=4).
            RuleFor(x => x.Method)
                .Must(m => System.Enum.IsDefined(typeof(PaymentMethod), m))
                .WithMessage("Phương thức thanh toán không hợp lệ.");
        }
    }
}
