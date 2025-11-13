using System.Threading; // เพิ่ม: สำหรับ CancellationToken
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Projects; // Namespace สำหรับ Request DTOs
using ProjectHub.Application.Dtos; // Namespace สำหรับ Response DTOs
using ProjectHub.Application.Features.Projects.CreateProject; // Namespace สำหรับ Create Command
using ProjectHub.Application.Features.Projects.DeleteProject; // Namespace สำหรับ Delete Command
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Features.Projects.GetAllProjects; // <-- *** [FIX 1] *** เพิ่ม Using นี้
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProjectHub.Application.Features.Projects.ToggleFavoriteProject; // <-- *** [FIX 2] *** เพิ่ม Using นี้
namespace ProjectHub.API.Controllers
{
    // [ApiController]: Attribute นี้เปิดใช้งานฟีเจอร์ต่างๆ ของ API Controller
    // เช่น การ Validate Model อัตโนมัติ, การแปลง Parameter Binding
    [ApiController]
    // [Route("api/[controller]")]: กำหนด URL พื้นฐานสำหรับ Controller นี้
    // "[controller]" จะถูกแทนที่ด้วยชื่อ Controller (Projects) -> /api/projects
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    
    public class ProjectsController : ControllerBase // สืบทอดจาก ControllerBase (สำหรับ API)
    {
        // --- Dependency Injection ---
        // readonly: หมายถึง field นี้จะถูกกำหนดค่าใน Constructor เท่านั้น
        private readonly IMediator _mediator; // Service สำหรับส่ง Command/Query ไปยัง Handler
        private readonly IMapper _mapper; // Service สำหรับแปลง Object (เช่น Request -> Command)
        private readonly ILogger<ProjectsController> _logger;


        // Constructor: รับ Services ที่ลงทะเบียนไว้ใน Program.cs เข้ามาใช้งาน
        public ProjectsController(IMediator mediator, IMapper mapper, ILogger<ProjectsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // --- Endpoint: สร้าง Project ใหม่ ---
        // [HttpPost]: ระบุว่าเมธอดนี้จะทำงานเมื่อมี HTTP POST request มาที่ Route พื้นฐาน (/api/projects)
        [HttpPost]
        // ActionResult<ProjectResponseDto>: ระบุว่าเมธอดนี้จะคืนค่า HTTP Status Code
        // และถ้าสำเร็จ (201 Created) จะมี ProjectResponseDto อยู่ใน Response Body ด้วย
        public async Task<ActionResult<ProjectResponseDto>> CreateProject(
            // [FromBody]: บอกให้ ASP.NET Core อ่านข้อมูลจาก Request Body (JSON)
            // แล้วแปลงเป็น Object CreateProjectRequest
            [FromBody] CreateProjectRequest request,
            // CancellationToken: ใช้สำหรับยกเลิก Operation (ถ้า Client ยกเลิก Request กลางคัน)
            CancellationToken ct
        )
        {
            // --- Input Validation (ทำใน Controller หรือ Command/Handler ก็ได้) ---
            // ตรวจสอบเบื้องต้นว่า Name ไม่ว่างเปล่า
            if (string.IsNullOrWhiteSpace(request.Name))
                // คืนค่า 400 Bad Request พร้อมข้อความ Error
                return BadRequest(new { Error = "Name is required." });

            // --- Mapping: Request -> Command ---
            // ใช้ AutoMapper แปลงข้อมูลจาก API Request (CreateProjectRequest)
            // ไปเป็น Application Command (CreateProjectCommand)
            // (AutoMapper จะอ่านกฎจาก ApiMappingProfile)
            var command = _mapper.Map<CreateProjectCommand>(request);
            // *** หมายเหตุ: UserId ควรจะมาจาก Request Body (CreateProjectRequest) ***
            // *** หรือถ้ามี Authentication ควรดึงมาจาก User.Claims ***
            // command.UserId = ???; // คุณต้องกำหนด UserId ให้ Command ตรงนี้

            try
            {
                // --- Logic Execution ---
                // ส่ง Command ไปให้ MediatR จัดการ (MediatR จะหา Handler ที่เหมาะสมมาทำงาน)
                // Handler (CreateProjectHandler) จะทำงาน, คุยกับ Repository, และคืนค่า DTO กลับมา
                ProjectResponseDto result = await _mediator.Send(command, ct);

                // --- Response ---
                // คืนค่า 201 Created พร้อม Location Header และ Response Body (DTO)
                // CreatedAtAction ต้องการ:
                // 1. ชื่อ Action ที่ใช้ดึงข้อมูล (ถ้ามี GetById) - ใส่ null ถ้าไม่มี
                // 2. Route Values สำหรับ Location Header (ID ของ Resource ที่สร้าง)
                // 3. Object ที่จะใส่ใน Response Body (DTO)
                return CreatedAtAction(null, new { id = result.ProjectId }, result);
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (เช่น ชื่อซ้ำ)
            {
                return BadRequest(new { Error = ex.Message }); // คืน 400
            }
            catch (Exception ex) // จับ Error อื่นๆ ที่ไม่คาดคิด
            {
                // ควร Log ex ไว้ด้วย
                return StatusCode(500, new { Error = "An unexpected error occurred." }); // คืน 500
            }
        }
        [HttpPut("{id}/toggle-favorite")]
        public async Task<IActionResult> ToggleFavorite(
            [FromRoute] int id,
            CancellationToken ct)
        {
            try
            {
                // 1. Controller "ฉลาด" ดึง UserId จาก Token (Claims)
                var userId = GetCurrentUserId();

                // 2. สร้าง Command (จาก Step 4)
                var command = new ToggleFavoriteProjectCommand
                {
                    ProjectId = id,
                    UserId = userId
                };

                // 3. ส่งให้ Handler (Handler จะพลิกค่า IsFavorite และอัปเดต UpdatedAt)
                ProjectResponseDto updatedProject = await _mediator.Send(command, ct);

                // 4. คืนค่า 200 OK พร้อม DTO ใหม่
                return Ok(updatedProject);
            }
            catch (UnauthorizedAccessException ex) // ถ้า Handler ตรวจสอบแล้วพบว่า "ไม่มีสิทธิ์"
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (ArgumentException ex) // ถ้า Handler โยน Error (เช่น Project ไม่เจอ)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // (ควร Log ex)
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjects(CancellationToken ct)
        {
            try
            {
                // 1. Controller "ฉลาด" ดึง UserId จาก Token (Claims)
                var userId = GetCurrentUserId();

                // 2. สร้าง Query และ "ยัด" UserId เข้าไป
                var query = new GetAllProjectsQuery { UserId = userId };

                // 3. ส่ง Query ที่ "สะอาด" (ไม่มี HttpContext) ไปให้ Handler
                var projects = await _mediator.Send(query, ct);

                // 4. คืนค่า 200 OK
                return Ok(projects);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // (ควร Log ex)
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        private int GetCurrentUserId()
        {
            // ดึง Claim "NameIdentifier" (ซึ่งปกติคือ User ID)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                // นี่คือ Error 401
                throw new UnauthorizedAccessException("User is not authenticated or UserId is invalid.");
            }
            return userId;
        }
        // --- Endpoint: แก้ไขชื่อ Project ---
        // [HttpPut("{id}")]: ระบุว่าเมธอดนี้จะทำงานเมื่อมี HTTP PUT request
        // มาที่ Route /api/projects/{id} (เช่น /api/projects/5)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] int id,
                                                   [FromBody] UpdateProjectRequest request,
                                                   CancellationToken ct)
        {
            var command = _mapper.Map<UpdateProjectCommand>(request);
            command.ProjectId = id;

            try
            {
                var updatedDto = await _mediator.Send(command, ct);
                return Ok(updatedDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to update project {ProjectId}", id);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Project {ProjectId} not found", id);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // บันทึก Exception ทั้งหมด พร้อม StackTrace
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace });
            }
        }


        // --- Endpoint: ลบ Project ---
        // [HttpDelete("{id}")]: ระบุว่าเมธอดนี้จะทำงานเมื่อมี HTTP DELETE request
        // มาที่ Route /api/projects/{id}

        // --- Endpoint: DELETE /api/rows/{id} ---
        [HttpDelete("{id}")] // รับ ID จาก URL Path
        public async Task<IActionResult> DeleteProject(
            [FromRoute] int id, // ดึง ID จาก Path
            CancellationToken ct
        ) // เพิ่ม ct
        {
            // สร้าง Command โดยตรงจาก ID ที่ได้จาก Route
            var command = new DeleteProjectCommand { ProjectId = id };

            try
            {
                // ส่ง Command ให้ MediatR (Handler จะคืน Unit)
                await _mediator.Send(command, ct); // เพิ่ม ct

                // คืน 204 No Content = สำเร็จ ไม่มีข้อมูลส่งกลับ
                return NoContent();
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (Row not found)
            {
                return NotFound(new { Error = ex.Message }); // คืน 404 Not Found
            }
            catch (Exception ex) // จับ Error อื่นๆ
            {
                Console.WriteLine($"Error deleting row {id}: {ex}"); // Log ง่ายๆ
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        // --- (Optional) Endpoint: ดึงข้อมูล Project ตาม ID ---
        // [HttpGet("{id}")]
        // public async Task<ActionResult<ProjectResponseDto>> GetProjectById(int id, CancellationToken ct)
        // {
        //     // สร้าง GetProjectByIdQuery และ Handler
        //     // var query = new GetProjectByIdQuery { ProjectId = id };
        //     // var result = await _mediator.Send(query, ct);
        //     // return result != null ? Ok(result) : NotFound();
        // }
    }
}
