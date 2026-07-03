using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.CreateDispute
{
    public class CreateDisputeCommand : IRequest<ApiResponse<long>>
    {
        [JsonIgnore]
        public long BookingId { get; set; }

        [JsonIgnore]
        public long CustomerId { get; set; }

        public string Reason { get; set; } = default!;
    }
}
