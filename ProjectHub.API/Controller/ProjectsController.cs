using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.DTOs; // <-- เพิ่มบรรทัดนี้
using ProjectHub.Application.Features.Projects.CreateProject; // <-- เพิ่มบรรทัดนี้

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // DTO สำหรับรับ Input (เราไม่ควรใช้ Command โดยตรงใน API)
        public record CreateProjectRequest(int UserId, string Name);

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            // 1. Map Request DTO ไปยัง Command
            var command = new CreateProjectCommand // <-- 'CreateProjectCommand' จะถูกพบ
            {
                UserId = request.UserId,
                Name = request.Name
            };

            try
            {
                // 2. ส่ง Command ไปยัง Handler
                ProjectResponseDto responseDto = await _mediator.Send(command); // <-- 'ProjectResponseDto' จะถูกพบ

                // 3. ส่ง 201 Created พร้อม DTO ที่ได้กลับมา
                return CreatedAtAction(
                    null, // (เรายังไม่มี GetById)
                    new { id = responseDto.ProjectId },
                    responseDto
                );
            }
            catch (System.ArgumentException ex) // (จับ Error ที่ Handler โยนมา)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (System.Exception ex)
            {
                // (จับ Error ทั่วไป)
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
