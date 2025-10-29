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

namespace ProjectHub.API.Controllers
{
    // [ApiController]: Attribute นี้เปิดใช้งานฟีเจอร์ต่างๆ ของ API Controller
    // เช่น การ Validate Model อัตโนมัติ, การแปลง Parameter Binding
    [ApiController]
    // [Route("api/[controller]")]: กำหนด URL พื้นฐานสำหรับ Controller นี้
    // "[controller]" จะถูกแทนที่ด้วยชื่อ Controller (Projects) -> /api/projects
    [Route("api/[controller]")] 
    public class ProjectsController : ControllerBase // สืบทอดจาก ControllerBase (สำหรับ API)
    {
        // --- Dependency Injection ---
        // readonly: หมายถึง field นี้จะถูกกำหนดค่าใน Constructor เท่านั้น
        private readonly IMediator _mediator; // Service สำหรับส่ง Command/Query ไปยัง Handler
        private readonly IMapper _mapper; // Service สำหรับแปลง Object (เช่น Request -> Command)

        // Constructor: รับ Services ที่ลงทะเบียนไว้ใน Program.cs เข้ามาใช้งาน
        public ProjectsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
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

        // --- Endpoint: แก้ไขชื่อ Project ---
        // [HttpPut("{id}")]: ระบุว่าเมธอดนี้จะทำงานเมื่อมี HTTP PUT request
        // มาที่ Route /api/projects/{id} (เช่น /api/projects/5)
        [HttpPut("{id}")]
        // IActionResult: ระบุว่าคืนค่า HTTP Status Code (อาจมี Body หรือไม่มีก็ได้)
        public async Task<IActionResult> UpdateProject(
            // [FromRoute]: บอกให้ดึงค่า Parameter 'id' มาจาก URL Path ({id})
            [FromRoute] int id,
            // [FromBody]: บอกให้ดึงข้อมูลใหม่ (NewName) มาจาก Request Body (JSON)
            [FromBody] UpdateProjectRequest request,
            CancellationToken ct
        )
        {
            // --- Mapping: Request -> Command ---
            // ใช้ AutoMapper แปลงข้อมูลจาก API Request (EditProjectRequest)
            // ไปเป็น Application Command (EditProjectCommand)
            var command = _mapper.Map<UpdateProjectCommand>(request);

            // *** สำคัญ: กำหนด ID ที่จะแก้ไข ให้กับ Command ***
            // ID มาจาก URL Path ไม่ใช่ Request Body
            command.ProjectId = id;

            try
            {
                // --- Logic Execution ---
                // ส่ง Command ไปให้ MediatR
                // Handler (EditProjectHandler) จะทำงาน, ดึงข้อมูล, แก้ไข, บันทึก
                // และคืนค่า DTO ที่อัปเดตแล้วกลับมา
                ProjectResponseDto updatedDto = await _mediator.Send(command, ct);

                // --- Response ---
                // คืนค่า 200 OK พร้อมข้อมูล Project ที่อัปเดตแล้ว
                return Ok(updatedDto);
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (เช่น ไม่เจอ Project ID)
            {
                return NotFound(new { Error = ex.Message }); // คืน 404 Not Found
            }
            catch (Exception ex) // จับ Error อื่นๆ
            {
                // ควร Log ex
                return StatusCode(500, new { Error = "An unexpected error occurred." }); // คืน 500
            }
        }

        // --- Endpoint: ลบ Project ---
        // [HttpDelete("{id}")]: ระบุว่าเมธอดนี้จะทำงานเมื่อมี HTTP DELETE request
        // มาที่ Route /api/projects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(
            // [FromRoute]: ดึงค่า Parameter 'id' มาจาก URL Path ({id})
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            // --- สร้าง Command ---
            // สร้าง Delete Command โดยตรง พร้อมกำหนด ID ที่จะลบ
            var command = new DeleteProjectCommand { ProjectId = id };

            try
            {
                // --- Logic Execution ---
                // ส่ง Command ไปให้ MediatR
                // Handler (DeleteProjectHandler) จะทำงาน, ดึงข้อมูล, สั่งลบ
                // (คืนค่า Unit.Value เพราะไม่มีข้อมูลส่งกลับ)
                await _mediator.Send(command, ct);

                // --- Response ---
                // คืนค่า 204 No Content = สำเร็จ แต่ไม่มีข้อมูลส่งกลับ
                return NoContent();
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (เช่น ไม่เจอ Project ID)
            {
                return NotFound(new { Error = ex.Message }); // คืน 404 Not Found
            }
            catch (Exception ex) // จับ Error อื่นๆ
            {
                // ควร Log ex
                return StatusCode(500, new { Error = "An unexpected error occurred." }); // คืน 500
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
