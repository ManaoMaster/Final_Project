using System;
using System.Threading.Tasks;
using AutoMapper; // เพิ่ม: ถ้าใช้ AutoMapper ใน API
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Rows;
using ProjectHub.Application.Dtos; // เพิ่ม: สำหรับ Response DTO
using ProjectHub.Application.Features.Rows.CreateRow;
using ProjectHub.Application.Features.Rows.UpdateRow; // เพิ่ม: สำหรับ Command

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
        [HttpPut("{id}")]
        // IActionResult: ระบุว่าคืนค่า HTTP Status Code (อาจมี Body หรือไม่มีก็ได้)
        public async Task<IActionResult> UpdateRow(
            // [FromRoute]: บอกให้ดึงค่า Parameter 'id' มาจาก URL Path ({id})
            [FromRoute] int id,
            // [FromBody]: บอกให้ดึงข้อมูลใหม่ (NewName) มาจาก Request Body (JSON)
            [FromBody] UpdateRowRequest request,
            CancellationToken ct
        )
        {
            // --- Mapping: Request -> Command ---
            // ใช้ AutoMapper แปลงข้อมูลจาก API Request (EditProjectRequest)
            // ไปเป็น Application Command (EditProjectCommand)
            var command = _mapper.Map<UpdateRowCommand>(request);

            // *** สำคัญ: กำหนด ID ที่จะแก้ไข ให้กับ Command ***
            // ID มาจาก URL Path ไม่ใช่ Request Body
            command.RowId = id;

            try
            {
                // --- Logic Execution ---
                // ส่ง Command ไปให้ MediatR
                // Handler (EditProjectHandler) จะทำงาน, ดึงข้อมูล, แก้ไข, บันทึก
                // และคืนค่า DTO ที่อัปเดตแล้วกลับมา
                RowResponseDto updatedDto = await _mediator.Send(command, ct);

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





    }
}
