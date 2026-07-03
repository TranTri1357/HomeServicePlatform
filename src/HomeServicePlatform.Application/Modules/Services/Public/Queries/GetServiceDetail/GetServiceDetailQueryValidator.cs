using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServiceDetail
{
    public class GetServiceDetailQueryValidator : AbstractValidator<GetServiceDetailQuery>
    {
        public GetServiceDetailQueryValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");
        }
    }
}
