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

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá dịch vụ phải lớn hơn 0.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Giá dịch vụ tối đa là 50.000.000đ.");
        }
    }
}
