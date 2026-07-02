using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Search.Dtos
{
    public class SearchResultDto
    {
        public List<CategoryResult> Categories { get; set; } = new();
        public List<ServiceResult> Services { get; set; } = new();
        public List<TaskerResult> Taskers { get; set; } = new();
    }
    public record CategoryResult(int Id, string Name, string IconUrl);
    public record ServiceResult(long Id, string Name);
    public record TaskerResult(long Id, string FullName, decimal RatingAvg, int TotalReviews);
}
