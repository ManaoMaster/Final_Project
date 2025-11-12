using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Tables;
using ProjectHub.Application.Dtos; // TableResponseDto
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Features.Tables.UpdateTable; // CreateTableCommand
using ProjectHub.Application.Features.Tables.GetAllTablesByProjectId; // <-- *** [FIX 1] *** เพิ่ม Using นี้
using System.Threading; // <-- [FIX 2] เพิ่ม Using นี้ (ถ้ายังไม่มี)

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route จะเป็น /api/tables
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; // เพิ่ม: ถ้าใช้ AutoMapper

        public TablesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper; // เพิ่ม

        }

        // DTO สำหรับรับ Input จาก Client (ควรอยู่ใน Contracts แต่ใช้ record ที่นี่เพื่อความง่าย)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTablesByProjectId(
                    [FromRoute] int projectId,
                    CancellationToken ct)
        {
            // 1. สร้าง Query จาก Route parameter
            var query = new GetAllTablesByProjectIdQuery { ProjectId = projectId };

            try
            {
                // 2. ส่ง Query ให้ Handler (Handler จะเช็คสิทธิ์ Project เอง)
                var tables = await _mediator.Send(query, ct);

                // 3. คืนค่า 200 OK
                return Ok(tables);
            }
            catch (UnauthorizedAccessException ex) // ถ้า Handler ตรวจสอบแล้วพบว่า "ไม่มีสิทธิ์"
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex) // ถ้า Handler โยน Error อื่นๆ (เช่น Project ไม่เจอ)
            {
                // (Handler ของเราโยน Exception ธรรมดา)
                return BadRequest(new { Error = ex.Message });
            }
        }
        // POST /api/tables
        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
        {
            // Map Request (API DTO) ไปยัง Command (Application DTO)
            // (ในโปรเจกต์จริง ควรใช้ AutoMapper ที่ ApiMappingProfile)
            var command = new CreateTableCommand
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                UseAutoIncrement = request.UseAutoIncrement,

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(
            [FromRoute] int id,
            [FromBody] UpdateTableRequest request,
            CancellationToken ct
        )
        {
            var command = _mapper.Map<UpdateTableCommand>(request);
            command.TableId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        // --- Endpoint: DELETE /api/rows/{id} ---
        [HttpDelete("{id}")] // รับ ID จาก URL Path
        public async Task<IActionResult> DeleteTable(
            [FromRoute] int id, // ดึง ID จาก Path
            CancellationToken ct
        ) // เพิ่ม ct
        {
            // สร้าง Command โดยตรงจาก ID ที่ได้จาก Route
            var command = new DeleteTableCommand { TableId = id };

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
                Console.WriteLine($"Error deleting table {id}: {ex}"); // Log ง่ายๆ
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
