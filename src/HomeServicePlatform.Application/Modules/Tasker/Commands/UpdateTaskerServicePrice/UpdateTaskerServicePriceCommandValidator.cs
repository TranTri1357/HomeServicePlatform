using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice
{
    public class UpdateTaskerServicePriceCommandValidator : AbstractValidator<UpdateTaskerServicePriceCommand>
    {
        public UpdateTaskerServicePriceCommandValidator()
        {
            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã thợ không hợp lệ.");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");

            // 🛡️ Xem ghi chú ở AddTaskerServiceCommandValidator: giá này được
            //    CreateBooking tra ra để tính tổng đơn, không được phép âm/0.
            RuleFor(x => x.NewPrice)
                .GreaterThan(0).WithMessage("Giá dịch vụ phải lớn hơn 0.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Giá dịch vụ tối đa là 50.000.000đ.");
        }
    }
}
