using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Projects.DeleteProject;
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
            CancellationToken ct
        )
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
        public async Task<IActionResult> EditProject(
            [FromBody] EditProjectRequest request,
            CancellationToken ct
        )
        {
            var command = _mapper.Map<EditProjectCommand>(request);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(
            [FromRoute] int projectId,
            CancellationToken ct
        ) // <-- รับ projectId จาก Route
        {
            // *** แก้ไข: ตรวจสอบว่า Assign ค่า projectId ถูกต้อง ***
            var command = new DeleteProjectCommand { ProjectId = projectId }; // <-- สร้าง Command พร้อม Assign ค่า

            try
            {
                // ส่ง Command (Handler คืน Unit)
                await _mediator.Send(command, ct); // <-- ส่ง ct
                return NoContent(); // 204 No Content = สำเร็จ
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (Project not found)
            {
                return NotFound(new { Error = ex.Message }); // คืน 404 Not Found
            }
            catch (Exception ex) // จับ Error อื่นๆ ที่ไม่คาดคิด
            {
                // Log the exception ex
                Console.WriteLine($"Error deleting project: {ex}"); // Log ง่ายๆ
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
