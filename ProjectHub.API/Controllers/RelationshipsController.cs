using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Relationships; 
using ProjectHub.Application.Dtos;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Relationships.CreateRelationship;
using ProjectHub.Application.Features.Relationships.DeleteRelationship;
using ProjectHub.Application.Features.Relationships.UpdateRelationship; 

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class RelationshipsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RelationshipsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateRelationship(
            [FromBody] CreateRelationshipRequest request,
            CancellationToken ct
        )
        {
            
            var command = _mapper.Map<CreateRelationshipCommand>(request);

            try
            {
                
                RelationshipResponseDto responseDto = await _mediator.Send(command, ct);

                
                return CreatedAtAction(
                    null, 
                    new { id = responseDto.RelationshipId },
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating relationship: {ex}");
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRelationship(
            [FromRoute] int id,
            [FromBody] UpdateRelationshipRequest request,
            CancellationToken ct
        )
        {
            
            var command = _mapper.Map<UpdateRelationshipCommand>(request);

            
            command.Id = id;

            
            await _mediator.Send(command, ct);

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRelationship(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            
            var command = new DeleteRelationshipCommand { Id = id };

            
            await _mediator.Send(command, ct);

            return NoContent(); 
        }
    }
}
