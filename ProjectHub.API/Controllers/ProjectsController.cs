using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Projects.EditProject;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ProjectsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectResponseDto>> CreateProject(
            [FromBody] CreateProjectRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { Error = "Name is required." });

            // ใช้ AutoMapper: API Request -> Application Command
            var command = _mapper.Map<CreateProjectCommand>(request);

            var result = await _mediator.Send(command, ct);

            // ไม่มี GetById ก็ใช้ Created + Location ตรงๆได้
            return Created($"/api/projects/{result.ProjectId}", result);
        }

        [HttpPut]
        public async Task<IActionResult> EditProject([FromBody] EditProjectRequest request, CancellationToken ct)
        {
            var command = _mapper.Map<EditProjectCommand>(request);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
