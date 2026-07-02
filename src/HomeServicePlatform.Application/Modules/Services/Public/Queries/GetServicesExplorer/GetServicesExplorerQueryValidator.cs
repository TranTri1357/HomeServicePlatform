using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServicesExplorer
{
    public class GetServicesExplorerQueryValidator : AbstractValidator<GetServicesExplorerQuery>
    {
        public GetServicesExplorerQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Trang hiện tại phải từ 1 trở lên.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Số lượng mục trên mỗi trang phải nằm trong khoảng từ 1 đến 100 để tránh quá tải.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).When(x => x.CategoryId.HasValue)
                .WithMessage("Mã danh mục không hợp lệ.");

            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
                .WithMessage("Giá tối thiểu không được là số âm.");

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue)
                .WithMessage("Giá tối đa không được là số âm.");

            RuleFor(x => x)
                .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
                .WithMessage("Giá tối thiểu phải nhỏ hơn hoặc bằng giá tối đa.");

            RuleFor(x => x.SortBy)
                .Must(sort => string.IsNullOrEmpty(sort) || new[] {"price_asc", "price_desc", "popular" }.Contains(sort.ToLower()))
                .WithMessage("Tiêu chí sắp xếp không hợp lệ.");
        }
    }
}
