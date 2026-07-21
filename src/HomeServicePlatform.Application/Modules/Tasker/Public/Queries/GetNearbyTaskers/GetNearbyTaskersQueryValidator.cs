using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetNearbyTaskers
{
    public class GetNearbyTaskersQueryValidator : AbstractValidator<GetNearbyTaskersQuery>
    {
        public GetNearbyTaskersQueryValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");

            RuleFor(x => x.CustomerLat)
                .InclusiveBetween(-90, 90)
                .WithMessage("Vĩ độ (Latitude) phải nằm trong khoảng -90 đến 90 độ.");

            RuleFor(x => x.CustomerLng)
                .InclusiveBetween(-180, 180)
                .WithMessage("Kinh độ (Longitude) phải nằm trong khoảng -180 đến 180 độ.");

            RuleFor(x => x.RadiusKm)
                .GreaterThan(0).WithMessage("Bán kính tìm kiếm phải lớn hơn 0.")
                .LessThanOrEqualTo(50).WithMessage("Hệ thống chỉ hỗ trợ tìm kiếm thợ trong bán kính tối đa 50km.");
        }
    }
}
