using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetUserDetail
{
    public class GetUserDetailValidator : AbstractValidator<GetUserDetailQuery>
    {
        public GetUserDetailValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Mã tài khoản không hợp lệ.");
        }
    }
}
