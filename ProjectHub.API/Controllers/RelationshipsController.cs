using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Relationships; // <-- ใช้ Request DTO ใหม่
using ProjectHub.Application.Dtos;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Relationships.CreateRelationship; // <-- ใช้ Command ใหม่
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/relationships
    public class RelationshipsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RelationshipsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // --- Endpoint: POST /api/relationships ---
        [HttpPost]
        public async Task<IActionResult> CreateRelationship(
            [FromBody] CreateRelationshipRequest request, 
            CancellationToken ct)
        {
            // 1. Map Request DTO -> Command
            var command = _mapper.Map<CreateRelationshipCommand>(request);

            try
            {
                // 2. ส่ง Command ไปยัง Handler
                RelationshipResponseDto responseDto = await _mediator.Send(command, ct);

                // 3. คืนค่า 201 Created
                return CreatedAtAction(
                    null, // (ยังไม่มี GetById)
                    new { id = responseDto.RelationshipId }, 
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                // จับ Error จาก Validation Logic (เช่น PK ไม่ใช่, Type ไม่ตรง)
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error creating relationship: {ex}"); 
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
