using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Categories.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Queries.GetCategoryDetail
{
    public record GetCategoryDetailQuery(int CategoryId) : IRequest<ApiResponse<CategoryDetailDto>>;
}
