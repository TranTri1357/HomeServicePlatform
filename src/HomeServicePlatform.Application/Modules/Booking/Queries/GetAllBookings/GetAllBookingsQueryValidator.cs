using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings
{
    public class GetAllBookingsQueryValidator : AbstractValidator<GetAllBookingsQuery>
    {
        public GetAllBookingsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1).WithMessage("Số trang (PageIndex) bắt buộc phải lớn hơn hoặc bằng 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Số lượng dòng trên mỗi trang (PageSize) tối thiểu phải là 1.")
                .LessThanOrEqualTo(100).WithMessage("Không được phép kéo quá 100 dòng dữ liệu trên một trang để bảo vệ hệ thống.");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(50).WithMessage("Từ khóa tìm kiếm (tên hoặc số điện thoại) không được vượt quá 50 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}
