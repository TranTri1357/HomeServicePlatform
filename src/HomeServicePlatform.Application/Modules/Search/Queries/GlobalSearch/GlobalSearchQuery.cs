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
        private string _keyword = string.Empty;

        public string Keyword
        {
            get => _keyword;
            set => _keyword = value?.Trim() ?? string.Empty;
        }
    }
}
