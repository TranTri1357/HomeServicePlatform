using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.CreateCategory
{
    public record CreateCategoryCommand(string Name, string Slug, string? IconUrl) : IRequest<ApiResponse<int>>;
}
