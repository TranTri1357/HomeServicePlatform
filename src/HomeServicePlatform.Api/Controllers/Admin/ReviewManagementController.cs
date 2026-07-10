using HomeServicePlatform.Application.Modules.Reviews.Admin.Commands.DeleteReview;
using HomeServicePlatform.Application.Modules.Reviews.Admin.Queries.GetAllReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [Route("api/admin/reviews")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ReviewManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ReviewManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllReviewsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _mediator.Send(new DeleteReviewCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
