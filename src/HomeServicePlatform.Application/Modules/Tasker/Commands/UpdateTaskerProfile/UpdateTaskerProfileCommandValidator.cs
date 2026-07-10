using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerProfile
{
    public class UpdateTaskerProfileCommandValidator : AbstractValidator<UpdateTaskerProfileCommand>
    {
        public UpdateTaskerProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Vui lòng nhập họ tên.")
                .MaximumLength(100).WithMessage("Họ tên không vượt quá 100 ký tự.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
                .Matches(@"^(03|05|07|08|09)\d{8}$")
                .WithMessage("Số điện thoại không đúng định dạng di động Việt Nam (10 số).");

            RuleFor(x => x.ExperienceYears)
                .GreaterThanOrEqualTo(0).WithMessage("Số năm kinh nghiệm không hợp lệ.")
                .LessThanOrEqualTo(70).WithMessage("Số năm kinh nghiệm không hợp lệ.");

            RuleFor(x => x.Bio)
                .MaximumLength(1000).WithMessage("Giới thiệu không vượt quá 1000 ký tự.");
        }
    }
}
