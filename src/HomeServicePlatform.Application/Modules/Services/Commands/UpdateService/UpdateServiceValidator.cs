using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Commands.UpdateService
{
    public class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
    {
        public UpdateServiceValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("ID dịch vụ không hợp lệ.");

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
