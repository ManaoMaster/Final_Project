using System;
using System.Threading.Tasks;
using AutoMapper; // เพิ่ม: ถ้าใช้ AutoMapper ใน API
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Rows;
using ProjectHub.Application.Dtos; // เพิ่ม: สำหรับ Response DTO
using ProjectHub.Application.Features.Rows.CreateRow; // เพิ่ม: สำหรับ Command

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // ใช้ /api/rows
    public class RowsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; // เพิ่ม: ถ้าใช้ AutoMapper

        // Inject IMediator และ IMapper (ถ้าใช้)
        public RowsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper; // เพิ่ม
        }

        // --- Endpoint: POST /api/rows ---
        [HttpPost]
        public async Task<IActionResult> CreateRow([FromBody] CreateRowRequest request)
        {
            // 1. Map Request DTO ไปยัง Command (ใช้ AutoMapper)
            var command = _mapper.Map<CreateRowCommand>(request);

            try
            {
                // 2. ส่ง Command ไปยัง MediatR (ซึ่งจะหา Handler มาทำงาน)
                RowResponseDto responseDto = await _mediator.Send(command);

                // 3. คืนค่า 201 Created พร้อม DTO ที่ได้กลับมา
                // (ใช้ GetById endpoint ถ้ามี, ถ้าไม่มี ใช้ null ไปก่อน)
                return CreatedAtAction(
                    null, // nameof(GetRowById) ถ้ามี Endpoint นี้
                    new { id = responseDto.RowId }, // ใช้ RowId จาก Response DTO
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                // จับ Error จาก Business Logic (เช่น Table ไม่เจอ, JSON Validation failed)
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) // จับ Error อื่นๆ ที่ไม่คาดคิด
            {
                // ควร Log exception ex ไว้ด้วย
                Console.WriteLine($"Error creating row: {ex}"); // Log ง่ายๆ ไปก่อน
                return StatusCode(
                    500,
                    new { Error = "An unexpected error occurred while creating the row." }
                );
            }
        }

        // --- (Optional) Endpoint: GET /api/rows/{id} ---
        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetRowById(int id)
        // {
        //     // สร้าง GetRowByIdQuery และ Handler
        // }

        // --- (Optional) Endpoint: GET /api/tables/{tableId}/rows ---
        // [HttpGet("/api/tables/{tableId}/rows")] // Route ที่แตกต่าง
        // public async Task<IActionResult> GetRowsByTableId(int tableId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        // {
        //     // สร้าง GetRowsByTableIdQuery และ Handler (พร้อม Pagination)
        // }
    }
}
