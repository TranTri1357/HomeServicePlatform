using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServicesExplorer
{
    public class GetServicesExplorerQuery : IRequest<ApiResponse<PagedResult<ServiceExplorerDto>>>
    {
        private string? _searchTerm;

        public string? SearchTerm
        {
            get => _searchTerm;
            set => _searchTerm = value?.Trim();
        }

        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public string? SortBy { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
