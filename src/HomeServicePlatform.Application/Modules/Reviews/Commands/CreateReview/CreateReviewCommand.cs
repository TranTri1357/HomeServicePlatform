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
        [JsonIgnore] // Ẩn khỏi Swagger Body, lấy từ URL route
        public long BookingItemId { get; set; }

        [JsonIgnore] // Ẩn khỏi Swagger Body, lấy từ Token
        public long CustomerId { get; set; }

        public short Rating { get; set; }
        public string? Comment { get; set; }
    }
}
