using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetNearbyTaskers;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerDetail;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerQuickInfo;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTopTaskers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskerDetail(long id)
        {
            var query = new GetTaskerDetailQuery { TaskerId = id };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>Danh sách dịch vụ + giá của thợ (cho khách chọn khi đặt lịch).</summary>
        [HttpGet("{id}/services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskerServices(long id)
        {
            var result = await _mediator.Send(new GetTaskerServiceOptionsQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>Khung giờ trống của thợ trong một ngày (cho khách chọn lịch hẹn).</summary>
        [HttpGet("{id}/availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(long id, [FromQuery] DateOnly date,
            [FromQuery] double? lat = null, [FromQuery] double? lng = null)
        {
            var result = await _mediator.Send(new GetTaskerAvailabilityQuery(id, date, lat, lng));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopTaskers([FromQuery] int limit = 5)
        {
            var query = new GetTopTaskersQuery { Limit = limit };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>Thợ nhận một dịch vụ, sắp theo đánh giá + phân trang "tải thêm" (mặc định 5/trang).</summary>
        [HttpGet("by-service/{serviceId:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskersByService(
            long serviceId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _mediator.Send(new GetServiceTaskersQuery(serviceId, pageIndex, pageSize));
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>Một thẻ thợ cho dịch vụ — để ghim thợ khách chọn sẵn lên đầu danh sách.</summary>
        [HttpGet("by-service/{serviceId:long}/tasker/{taskerId:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceTaskerCard(long serviceId, long taskerId)
        {
            var result = await _mediator.Send(new GetServiceTaskerCardQuery(serviceId, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("nearby")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNearbyTaskers([FromQuery] long serviceId, [FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 10)
        {
            var query = new GetNearbyTaskersQuery
            {
                ServiceId = serviceId,
                CustomerLat = lat,
                CustomerLng = lng,
                RadiusKm = radius
            };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}/service/{serviceId}/quick-info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskerQuickInfo(long id, long serviceId)
        {
            var query = new GetTaskerQuickInfoQuery
            {
                TaskerId = id,
                ServiceId = serviceId
            };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
