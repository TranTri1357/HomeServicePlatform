using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTopTaskers
{
    public class GetTopTaskersQueryValidator : AbstractValidator<GetTopTaskersQuery>
    {
        public GetTopTaskersQueryValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Số lượng thợ cần lấy phải lớn hơn 0.")
                .LessThanOrEqualTo(10).WithMessage("Chỉ được phép lấy tối đa 10 thợ nổi bật.");
        }
    }
}
