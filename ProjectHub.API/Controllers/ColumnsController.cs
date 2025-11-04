using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Columns; // <-- 1. ใช้ DTO ที่ถูกต้องจาก Contracts
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Features.Columns.DeleteColumn;
using ProjectHub.Application.Features.Columns.UpdateColumn; // <-- 2. เพิ่ม Using นี้สำหรับ Update
using System.Threading;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId; // <-- 3. เพิ่ม Using นี้สำหรับ CancellationToken

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColumnsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ColumnsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // --- (ลบ record CreateColumnRequest และ CreateColumnRequestPayload ที่ซ้ำซ้อนทิ้งไป) ---

        // --- Endpoint: POST /api/columns ---
        // (เราไม่ต้องใช้ {tableId} ใน Route แล้ว เพราะมันอยู่ใน Request Body)
        [HttpPost]
        public async Task<IActionResult> CreateColumn(
            [FromBody] CreateColumnRequest request, // <-- 4. เปลี่ยนมารับ DTO ที่ถูกต้อง
            CancellationToken ct // <-- เพิ่ม ct
        )
        {
            // 5. ใช้ AutoMapper (แบบนี้ FormulaDefinition จะถูกแมปไปด้วย)
            var command = _mapper.Map<CreateColumnCommand>(request);

            try
            {
                // 6. ส่ง Command ไปยัง MediatR
                ColumnResponseDto responseDto = await _mediator.Send(command, ct); // <-- ส่ง ct

                // 7. คืนค่า 201 Created
                return CreatedAtAction(
                    null,
                    new { id = responseDto.ColumnId },
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateColumn(
            [FromRoute] int id,
            [FromBody] UpdateColumnRequest request, // (DTO นี้ต้องมีอยู่ใน Contracts/Columns)
            CancellationToken ct
        )
        {
            var command = _mapper.Map<UpdateColumnCommand>(request);
            command.ColumnId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result); // (สมมติว่า Handler คืน DTO ที่อัปเดตแล้ว)
        }

        // --- Endpoint: DELETE /api/columns/{id} ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColumn(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var command = new DeleteColumnCommand { ColumnId = id };

            try
            {
                await _mediator.Send(command, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting column {id}: {ex}"); // (แก้จาก row เป็น column)
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        [HttpGet("table/{tableId}/primary")]
        public async Task<IActionResult> GetPrimaryColumnsByTableId(int tableId)
        {
            var query = new GetPrimaryColumnsByTableIdQuery { TableId = tableId };
            var primaryColumns = await _mediator.Send(query);
            return Ok(primaryColumns);
        }

        // --- (Optional) Endpoint: GET /api/columns/{id} ---
        // (ยังไม่ได้สร้าง Handler)
    }
}