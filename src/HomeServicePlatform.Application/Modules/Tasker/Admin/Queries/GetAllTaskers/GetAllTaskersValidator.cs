using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetAllTaskers
{
    public class GetAllTaskersValidator : AbstractValidator<GetAllTaskersQuery>
    {
        public GetAllTaskersValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("Trang hiện tại phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Kích thước trang không hợp lệ.")
                .LessThanOrEqualTo(100).WithMessage("Không lấy quá 100 bản ghi mỗi trang.");
        }
    }
}
