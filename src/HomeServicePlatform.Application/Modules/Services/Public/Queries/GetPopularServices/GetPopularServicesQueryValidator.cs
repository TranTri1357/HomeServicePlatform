using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetPopularServices
{
    public class GetPopularServicesQueryValidator : AbstractValidator<GetPopularServicesQuery>
    {
        public GetPopularServicesQueryValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Giới hạn phải là một số dương.")
                .LessThanOrEqualTo(10).WithMessage("Chỉ được phép lấy tối đa 10 dịch vụ.");
        }
    }
}
