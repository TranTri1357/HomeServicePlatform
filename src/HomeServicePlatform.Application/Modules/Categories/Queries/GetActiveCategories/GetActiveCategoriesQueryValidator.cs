using FluentValidation;
using HomeServicePlatform.Application.Modules.Services.Public.Queries.GetPopularServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Queries.GetActiveCategories
{
    public class GetActiveCategoriesQueryValidator : AbstractValidator<GetActiveCategoriesQuery>
    {
        public GetActiveCategoriesQueryValidator() 
        {
            RuleFor(x => x.Limit)
            .GreaterThan(0).When(x => x.Limit.HasValue).WithMessage("Giới hạn phải là một số dương.")
            .LessThanOrEqualTo(10).When(x => x.Limit.HasValue).WithMessage("Chỉ được phép lấy tối đa 10 danh mục."); ;
        }
    }
}
