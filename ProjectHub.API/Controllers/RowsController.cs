using System;
using System.Threading.Tasks;
using AutoMapper; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Rows;
using ProjectHub.Application.Dtos; 
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Rows.CreateRow;
using ProjectHub.Application.Features.Rows.GetRowsByTableId;
using ProjectHub.Application.Features.Rows.UpdateRow; 

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class RowsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; 

        
        public RowsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper; 
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateRow([FromBody] CreateRowRequest request)
        {
            
            var command = _mapper.Map<CreateRowCommand>(request);

            try
            {
                
                RowResponseDto responseDto = await _mediator.Send(command);

                
                
                return CreatedAtAction(
                    null, 
                    new { id = responseDto.RowId }, 
                    responseDto
                );
            }
            catch (ArgumentException ex)
            {
                
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) 
            {
                
                Console.WriteLine($"Error creating row: {ex}"); 
                return StatusCode(
                    500,
                    new { Error = "An unexpected error occurred while creating the row." }
                );
            }
        }
        
        
        [HttpGet("table/{tableId:int}")]
        public async Task<IActionResult> GetRowsByTableId(
            [FromRoute] int tableId,
            CancellationToken ct)
        {
            
            var query = new GetRowsByTableIdQuery { TableId = tableId };

            
            var result = await _mediator.Send(query, ct); 

            
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> UpdateRow(
            
            [FromRoute] int id,
            
            [FromBody] UpdateRowRequest request,
            CancellationToken ct
        )
        {
            
            
            
            var command = _mapper.Map<UpdateRowCommand>(request);

            
            
            command.RowId = id;

            try
            {
                
                
                
                
                RowResponseDto updatedDto = await _mediator.Send(command, ct);

                
                
                return Ok(updatedDto);
            }
            catch (ArgumentException ex) 
            {
                return NotFound(new { Error = ex.Message }); 
            }
            catch (Exception ex) 
            {
                
                return StatusCode(500, new { Error = "An unexpected error occurred." }); 
            }
        }

        
        [HttpDelete("{id}")] 
        public async Task<IActionResult> DeleteRow(
            [FromRoute] int id, 
            CancellationToken ct
        ) 
        {
            
            var command = new DeleteRowCommand { RowId = id };

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
                Console.WriteLine($"Error deleting row {id}: {ex}"); 
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
