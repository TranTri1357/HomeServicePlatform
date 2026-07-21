using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Reviews.Commands.CreateReview
{
    public class CreateReviewCommand : IRequest<ApiResponse<long>>
    {
        [JsonIgnore]
        public long BookingItemId { get; set; }

        [JsonIgnore]
        public long CustomerId { get; set; }

        public short Rating { get; set; }
        public string? Comment { get; set; }
    }
}
