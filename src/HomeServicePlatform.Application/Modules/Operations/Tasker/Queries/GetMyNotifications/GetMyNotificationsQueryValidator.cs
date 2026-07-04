using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetMyNotifications
{
    public class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
    {
        public GetMyNotificationsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Số lượng trên mỗi trang phải lớn hơn 0.")
                .LessThanOrEqualTo(20).WithMessage("Chỉ được phép lấy tối đa 20 thông báo mỗi lần tải.");
        }
    }
}
