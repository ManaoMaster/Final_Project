using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.DTOs; // TableResponseDto
using ProjectHub.Application.Features.Tables.CreateTable; // CreateTableCommand
using System;
using System.Threading.Tasks;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route จะเป็น /api/tables
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Inject MediatR
        public TablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // DTO สำหรับรับ Input จาก Client (ควรอยู่ใน Contracts แต่ใช้ record ที่นี่เพื่อความง่าย)
        public record CreateTableRequest(int ProjectId, string Name);

        // POST /api/tables
        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
        {
            // Map Request (API DTO) ไปยัง Command (Application DTO)
            // (ในโปรเจกต์จริง ควรใช้ AutoMapper ที่ ApiMappingProfile)
            var command = new CreateTableCommand
            {
                ProjectId = request.ProjectId,
                Name = request.Name
            };

            try
            {
                // ส่ง Command ไปยัง Handler ผ่าน MediatR
                TableResponseDto response = await _mediator.Send(command);

                // คืนค่า 201 Created พร้อม DTO ที่ Handler ส่งกลับมา
                // GetById ยังไม่มี ใช้ null ไปก่อน
                return CreatedAtAction(null, new { id = response.TableId }, response);
            }
            catch (ArgumentException ex) // จับ Error ที่ Handler โยนมา (ชื่อซ้ำ, Project ไม่เจอ)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) // จับ Error อื่นๆ ที่ไม่คาดคิด
            {
                // Log Error จริงๆ ที่นี่
                Console.WriteLine($"Error creating table: {ex}"); // แสดงใน Console ชั่วคราว
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
