using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Features.Projects.GetAllProjects;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProjectHub.Application.Features.Projects.ToggleFavoriteProject;
namespace ProjectHub.API.Controllers
{


    [ApiController]


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]

    public class ProjectsController : ControllerBase
    {


        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectsController> _logger;



        public ProjectsController(IMediator mediator, IMapper mapper, ILogger<ProjectsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }



        [HttpPost]


        public async Task<ActionResult<ProjectResponseDto>> CreateProject(


            [FromBody] CreateProjectRequest request,

            CancellationToken ct
        )
        {


            if (string.IsNullOrWhiteSpace(request.Name))

                return BadRequest(new { Error = "Name is required." });





            var command = _mapper.Map<CreateProjectCommand>(request);




            try
            {



                ProjectResponseDto result = await _mediator.Send(command, ct);







                return CreatedAtAction(null, new { id = result.ProjectId }, result);
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
        [HttpPut("{id}/toggle-favorite")]
        public async Task<IActionResult> ToggleFavorite(
            [FromRoute] int id,
            CancellationToken ct)
        {
            try
            {

                var userId = GetCurrentUserId();


                var command = new ToggleFavoriteProjectCommand
                {
                    ProjectId = id,
                    UserId = userId
                };


                ProjectResponseDto updatedProject = await _mediator.Send(command, ct);


                return Ok(updatedProject);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
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
        [HttpGet]
        public async Task<IActionResult> GetAllProjects(CancellationToken ct)
        {
            try
            {

                var userId = GetCurrentUserId();


                var query = new GetAllProjectsQuery { UserId = userId };


                var projects = await _mediator.Send(query, ct);


                return Ok(projects);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        private int GetCurrentUserId()
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {

                throw new UnauthorizedAccessException("User is not authenticated or UserId is invalid.");
            }
            return userId;
        }


        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] int id,
                                                   [FromBody] UpdateProjectRequest request,
                                                   CancellationToken ct)
        {
            var command = _mapper.Map<UpdateProjectCommand>(request);
            command.ProjectId = id;

            try
            {
                var updatedDto = await _mediator.Send(command, ct);
                return Ok(updatedDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to update project {ProjectId}", id);
                return Unauthorized(new { Error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Project {ProjectId} not found", id);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace });
            }
        }


        
        
        

        
        [HttpDelete("{id}")] 
        public async Task<IActionResult> DeleteProject(
            [FromRoute] int id, 
            CancellationToken ct
        ) 
        {
            
            var command = new DeleteProjectCommand { ProjectId = id };

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
