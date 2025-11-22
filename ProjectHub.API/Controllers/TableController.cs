using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Tables;
using ProjectHub.Application.Dtos; 
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Features.Tables.UpdateTable; 
using ProjectHub.Application.Features.Tables.GetAllTablesByProjectId; 
using System.Threading; 

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class TablesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; 

        public TablesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper; 

        }

        
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTablesByProjectId(
                    [FromRoute] int projectId,
                    CancellationToken ct)
        {
            
            var query = new GetAllTablesByProjectIdQuery { ProjectId = projectId };

            try
            {
                
                var tables = await _mediator.Send(query, ct);

                
                return Ok(tables);
            }
            catch (UnauthorizedAccessException ex) 
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex) 
            {
                
                return BadRequest(new { Error = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
        {
            
            
            var command = new CreateTableCommand
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                UseAutoIncrement = request.UseAutoIncrement,

            };

            try
            {
                
                TableResponseDto response = await _mediator.Send(command);

                
                
                return CreatedAtAction(null, new { id = response.TableId }, response);
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex) 
            {
                
                Console.WriteLine($"Error creating table: {ex}"); 
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
        
        [HttpDelete("{id}")] 
        public async Task<IActionResult> DeleteTable(
            [FromRoute] int id, 
            CancellationToken ct
        ) 
        {
            
            var command = new DeleteTableCommand { TableId = id };

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
                Console.WriteLine($"Error deleting table {id}: {ex}"); 
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
