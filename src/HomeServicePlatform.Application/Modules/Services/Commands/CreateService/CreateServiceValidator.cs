using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Commands.CreateService
{
    public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Danh mục không hợp lệ.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên dịch vụ không được để trống.")
                .MaximumLength(200).WithMessage("Tên dịch vụ không được vượt quá 200 ký tự.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Thời lượng phải lớn hơn 0 phút.");
        }
    }
}
