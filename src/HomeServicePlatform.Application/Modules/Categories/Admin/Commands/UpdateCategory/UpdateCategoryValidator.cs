using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.UpdateCategory
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("ID loại dịch vụ không hợp lệ.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên loại dịch vụ không được để trống.")
                .MaximumLength(100).WithMessage("Tên loại không được vượt quá 100 ký tự.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Đường dẫn (Slug) không được để trống.")
                .Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ được chứa chữ cái thường, số và dấu gạch ngang.");
        }
    }
}
