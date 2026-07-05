using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.DeleteCategory
{
    public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("ID loại dịch vụ không hợp lệ.");
        }
    }
}
