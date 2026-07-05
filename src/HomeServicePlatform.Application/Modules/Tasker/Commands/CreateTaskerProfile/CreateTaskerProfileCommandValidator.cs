using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommandValidator : AbstractValidator<CreateTaskerProfileCommand>
    {
        public CreateTaskerProfileCommandValidator()
        {
            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Vui lòng nhập giới thiệu bản thân.")
                .MaximumLength(1000).WithMessage("Giới thiệu không vượt quá 1000 ký tự.");

            RuleFor(x => x.ExperienceYears)
                .GreaterThanOrEqualTo(0).WithMessage("Số năm kinh nghiệm không hợp lệ.");

            RuleFor(x => x.Bio).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        }
    }
}
