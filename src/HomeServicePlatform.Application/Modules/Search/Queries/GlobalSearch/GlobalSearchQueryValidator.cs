using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Search.Queries.GlobalSearch
{
    public class GlobalSearchQueryValidator : AbstractValidator<GlobalSearchQuery>
    {
        public GlobalSearchQueryValidator()
        {
            RuleFor(x => x.Keyword)
                .NotEmpty().WithMessage("Từ khóa tìm kiếm không được để trống.");
        }
    }
}
