using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService
{
    public class AddTaskerServiceCommandValidator : AbstractValidator<AddTaskerServiceCommand>
    {
        public AddTaskerServiceCommandValidator()
        {
            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã thợ không hợp lệ.");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");

            // 🛡️ Giá niêm yết là nguồn server dùng để tính tổng đơn và hoa hồng
            //    (CreateBooking tra giá từ TaskerServicePrices). Giá âm/0 sẽ làm
            //    tổng đơn âm và phá luôn phần tính hoa hồng — phải chặn tại đây.
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá dịch vụ phải lớn hơn 0.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Giá dịch vụ tối đa là 50.000.000đ.");
        }
    }
}
