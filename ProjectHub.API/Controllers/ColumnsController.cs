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
             [FromBody] CreateColumnRequest request, // <-- 1. รับ Request ที่อัปเกรดแล้ว
             CancellationToken ct
         )
        {
            // --- [FIX] ---
            // เราจะสร้าง Command ด้วยมือ เพื่อจัดการ Logic ไก่กับไข่
            // (ลบ var command = _mapper.Map<CreateColumnCommand>(request); ทิ้งไป)

            var command = new CreateColumnCommand
            {
                TableId = request.TableId,
                Name = request.Name,
                DataType = request.DataType,
                IsNullable = request.IsNullable,
                IsPrimary = request.IsPrimary,
                FormulaDefinition = request.FormulaDefinition,
                LookupTargetColumnId = request.LookupTargetColumnId,

                // 2. ส่ง "ไข่" (ID ที่มีอยู่)
                LookupRelationshipId = request.LookupRelationshipId,

                // 3. แปลง "ไก่" (ข้อมูลใหม่) จาก Request -> Command
                NewRelationship = (request.NewRelationship == null) ? null : new NewRelationshipData
                {
                    PrimaryTableId = request.NewRelationship.PrimaryTableId,
                    PrimaryColumnId = request.NewRelationship.PrimaryColumnId,
                    ForeignTableId = request.NewRelationship.ForeignTableId,
                    ForeignColumnId = request.NewRelationship.ForeignColumnId
                }
            };
            // --- [End of FIX] ---

            try
            {
                // 4. ส่ง Command ที่เราสร้างเอง ไปยัง MediatR
                ColumnResponseDto responseDto = await _mediator.Send(command, ct);

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
            // vvv --- [FIX] เพิ่ม try...catch บล็อกนี้ --- vvv
            try
            {
                var query = new GetPrimaryColumnsByTableIdQuery { TableId = tableId };
                var primaryColumns = await _mediator.Send(query);
                return Ok(primaryColumns);
            }
            catch (UnauthorizedAccessException ex) // ถ้า "ยาม" (Security) ทำงาน
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex) // ถ้า Error อื่นๆ (เช่น Table ไม่เจอ)
            {
                return BadRequest(new { Error = ex.Message });
            }
            // ^^^ --- [สิ้นสุดการแก้] --- ^^^
        }
        [HttpGet("table/{tableId}")] // <-- นี่คือ "ประตู" ใหม่
        public async Task<IActionResult> GetColumnsByTableId(int tableId)
        {
            // (สร้าง Query (Source 184) ที่ Handler (Source 183) รอรับ)
            var query = new GetColumnsByTableIdQuery { TableId = tableId };

            try
            {
                var columns = await _mediator.Send(query);
                return Ok(columns);
            }
            catch (UnauthorizedAccessException ex) // ถ้า "ยาม" (Security) ทำงาน
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex) // ถ้า Error อื่นๆ (เช่น Table ไม่เจอ)
            {
                // (ใน Service ของเรา เรา throw Exception ธรรมดา)
                return BadRequest(new { Error = ex.Message });
            }
        }


        // --- (Optional) Endpoint: GET /api/columns/{id} ---
        // (ยังไม่ได้สร้าง Handler)
    }
}