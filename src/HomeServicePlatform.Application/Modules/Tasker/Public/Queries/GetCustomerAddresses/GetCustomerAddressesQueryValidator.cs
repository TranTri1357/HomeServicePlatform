using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetCustomerAddresses
{
    public class GetCustomerAddressesQueryValidator : AbstractValidator<GetCustomerAddressesQuery>
    {
        public GetCustomerAddressesQueryValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Mã khách hàng không hợp lệ.");
        }
    }
}
