using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetAllUsers
{
    public class GetAllUsersValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("Trang hiện tại phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Kích thước trang phải lớn hơn 0.")
                .LessThanOrEqualTo(100).WithMessage("Không được lấy quá 100 bản ghi mỗi trang để đảm bảo hiệu năng máy chủ.");
        }
    }
}
