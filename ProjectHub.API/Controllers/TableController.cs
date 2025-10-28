using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Tables;
using ProjectHub.Application.Dtos; // TableResponseDto
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.UpdateTable; // CreateTableCommand

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

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteTable(
        //     [FromRoute] int id,
        //     CancellationToken ct
        // )
        // {
        //     var command = new DeleteTableCommand { TableId = id };
        //     var result = await _mediator.Send(command, ct);
        //     return Ok(result);
        // }
    }
}
