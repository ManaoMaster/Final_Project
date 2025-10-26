using System;
using System.Threading.Tasks;
using AutoMapper; // เพิ่ม: ถ้าใช้ AutoMapper ใน API
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn; // เพิ่ม: สำหรับ Command

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // ใช้ /api/columns
    public class ColumnsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; // เพิ่ม: ถ้าใช้ AutoMapper

        // Inject IMediator และ IMapper
        public ColumnsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper; // เพิ่ม
        }

        // --- DTO สำหรับรับ Request Body ---
        // ควรสร้างไฟล์แยกใน ProjectHub.API/Contracts/Columns/CreateColumnRequest.cs
        // แต่เพื่อความง่าย ใส่เป็น record ที่นี่ก่อนได้
        public record CreateColumnRequest(
            [FromRoute] int TableId, // สมมติว่า TableId มาจาก Route Parameter
            [FromBody] CreateColumnRequestPayload Payload // ข้อมูล Column มาจาก Body
        );
        public record CreateColumnRequestPayload(
            string Name,
            string DataType,
            bool IsPrimary = false, // กำหนด Default
            bool IsNullable = true // กำหนด Default
        );


        // --- Endpoint: POST /api/columns/{tableId} ---
        [HttpPost("{tableId}")] // รับ TableId จาก URL
        public async Task<IActionResult> CreateColumn([FromRoute] int tableId, [FromBody] CreateColumnRequestPayload payload)
        {
            // 1. สร้าง Command จาก Request DTO และ Route Parameter
             var command = new CreateColumnCommand
             {
                 TableId = tableId, // มาจาก Route
                 Name = payload.Name,
                 DataType = payload.DataType,
                 IsPrimary = payload.IsPrimary,
                 IsNullable = payload.IsNullable
             };

            // // หรือถ้าใช้ AutoMapper (ต้องสร้าง Request DTO ที่สมบูรณ์ก่อน)
            // var command = _mapper.Map<CreateColumnCommand>(request);

            try
            {
                // 2. ส่ง Command ไปยัง MediatR (ซึ่งจะหา Handler มาทำงาน)
                ColumnResponseDto responseDto = await _mediator.Send(command);

                // 3. คืนค่า 201 Created พร้อม DTO ที่ได้กลับมา
                // (ใช้ GetById endpoint ถ้ามี, ถ้าไม่มี ใช้ null ไปก่อน)
                return CreatedAtAction(
                    null, // nameof(GetColumnById) ถ้ามี Endpoint นี้
                    new { id = responseDto.ColumnId },
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                // จับ Error จาก Business Logic (เช่น ชื่อซ้ำ, Table ไม่เจอ)
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) // จับ Error อื่นๆ ที่ไม่คาดคิด
            {
                // Log the exception ex
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        // --- (Optional) Endpoint: GET /api/columns/{id} ---
        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetColumnById(int id)
        // {
        //     // สร้าง GetColumnByIdQuery และ Handler
        //     // var query = new GetColumnByIdQuery { ColumnId = id };
        //     // var result = await _mediator.Send(query);
        //     // return result != null ? Ok(result) : NotFound();
        // }
    }
}
