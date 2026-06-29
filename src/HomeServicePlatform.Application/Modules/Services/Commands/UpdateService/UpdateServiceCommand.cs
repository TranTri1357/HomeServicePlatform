using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Commands.UpdateService
{
    public class UpdateServiceCommand : IRequest<ApiResponse<bool>>
    {
        public long ServiceId { get; set; } // Sẽ được gán thủ công từ URL ở Controller
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
