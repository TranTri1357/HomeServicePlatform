using HomeServicePlatform.Application.Modules.Categories.Admin.Commands.CreateCategory;
using HomeServicePlatform.Application.Modules.Categories.Admin.Commands.DeleteCategory;
using HomeServicePlatform.Application.Modules.Categories.Admin.Commands.UpdateCategory;
using HomeServicePlatform.Application.Modules.Categories.Admin.Queries.GetAllCategories;
using HomeServicePlatform.Application.Modules.Categories.Admin.Queries.GetCategoryDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCategoriesQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var result = await _mediator.Send(new GetCategoryDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryCommand command)
        {
            command.CategoryId = id;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
