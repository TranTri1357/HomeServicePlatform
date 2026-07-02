using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Categories.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Queries.GetActiveCategories
{
    public class GetActiveCategoriesQuery : IRequest<ApiResponse<List<CategoryDto>>>
    {
        public int? Limit { get; set; }
    }
}
