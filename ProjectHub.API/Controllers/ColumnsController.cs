using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Columns; 
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Features.Columns.DeleteColumn;
using ProjectHub.Application.Features.Columns.UpdateColumn; 
using System.Threading;
using ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId; 

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

        

        
        
        [HttpPost]
        public async Task<IActionResult> CreateColumn(
             [FromBody] CreateColumnRequest request, 
             CancellationToken ct
         )
        {
            
            
            

            var command = new CreateColumnCommand
            {
                TableId = request.TableId,
                Name = request.Name,
                DataType = request.DataType,
                IsNullable = request.IsNullable,
                IsPrimary = request.IsPrimary,
                FormulaDefinition = request.FormulaDefinition,
                LookupTargetColumnId = request.LookupTargetColumnId,

                
                LookupRelationshipId = request.LookupRelationshipId,

                
                NewRelationship = (request.NewRelationship == null) ? null : new NewRelationshipData
                {
                    PrimaryTableId = request.NewRelationship.PrimaryTableId,
                    PrimaryColumnId = request.NewRelationship.PrimaryColumnId,
                    ForeignTableId = request.NewRelationship.ForeignTableId,
                    ForeignColumnId = request.NewRelationship.ForeignColumnId
                }
            };
            

            try
            {
                
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
            [FromBody] UpdateColumnRequest request, 
            CancellationToken ct
        )
        {
            var command = _mapper.Map<UpdateColumnCommand>(request);
            command.ColumnId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result); 
        }

        
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
                Console.WriteLine($"Error deleting column {id}: {ex}"); 
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        [HttpGet("table/{tableId}/primary")]
        public async Task<IActionResult> GetPrimaryColumnsByTableId(int tableId)
        {
            
            try
            {
                var query = new GetPrimaryColumnsByTableIdQuery { TableId = tableId };
                var primaryColumns = await _mediator.Send(query);
                return Ok(primaryColumns);
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
        [HttpGet("table/{tableId}")] 
        public async Task<IActionResult> GetColumnsByTableId(int tableId)
        {
            
            var query = new GetColumnsByTableIdQuery { TableId = tableId };

            try
            {
                var columns = await _mediator.Send(query);
                return Ok(columns);
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


        
        
    }
}