using HomeServicePlatform.Application.Modules.Services.Admin.Commands.CreateService;
using HomeServicePlatform.Application.Modules.Services.Admin.Commands.DeleteService;
using HomeServicePlatform.Application.Modules.Services.Admin.Commands.UpdateService;
using HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetAllServices;
using HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetServiceDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/services")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public class ServiceManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAllServices([FromQuery] GetAllServicesQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceDetail(long id)
        {
            var result = await _mediator.Send(new GetServiceDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(long id, [FromBody] UpdateServiceCommand command)
        {
            command.ServiceId = id;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(long id)
        {
            var result = await _mediator.Send(new DeleteServiceCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
