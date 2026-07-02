using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Search.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Search.Queries
{
    public class GlobalSearchQuery : IRequest<ApiResponse<SearchResultDto>>
    {
        public string Keyword { get; set; } = string.Empty;
    }
}
