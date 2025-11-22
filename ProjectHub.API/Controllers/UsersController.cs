using System.Security.Claims;
using System.Threading;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Users;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Users.ChangePassword;
using ProjectHub.Application.Features.Users.EditProfile;
using ProjectHub.Application.Features.Users.Login;
using ProjectHub.Application.Features.Users.Register;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Features.Users.ForgotPassword;
using ProjectHub.Application.Features.Users.ResetPassword;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUserRepository _users; 

        public UsersController(IMediator mediator, IMapper mapper, IUserRepository users)
        {
            _mediator = mediator;
            _mapper = mapper;
            _users = users; 
        }

        
        
        
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest req,
            CancellationToken ct
        )
        {
            var sub =
                User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub is null)
                return Unauthorized();

            var cmd = new ChangePasswordCommand
            {
                UserId = int.Parse(sub),
                CurrentPassword = req.CurrentPassword,
                NewPassword = req.NewPassword,
            };

            try
            {
                await _mediator.Send(cmd, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        
        
        
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(
            [FromBody] RegisterUserRequest request,
            CancellationToken ct
        )
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var userDto = await _mediator.Send(command, ct);
            return Created($"/api/users/{userDto.UserId}", userDto);
        }

        
        
        
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(
            [FromBody] LoginRequest req,
            CancellationToken ct
        )
        {
            try
            {
                var token = await _mediator.Send(
                    new LoginCommand { Email = req.Email, Password = req.Password },
                    ct
                );
                return Ok(token);
            }
            catch (ArgumentException)
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }
        }

        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest req,
            CancellationToken ct)
        {
            await _mediator.Send(new ForgotPasswordCommand { Email = req.Email }, ct);
            
            return Ok();
        }

        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest req,
            CancellationToken ct)
        {
            try
            {
                await _mediator.Send(
                    new ResetPasswordCommand
                    {
                        Token = req.Token,
                        NewPassword = req.NewPassword,
                    },
                    ct
                );

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        
        
        
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me(CancellationToken ct)
        {
            
            var sub =
                User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(sub))
                return Unauthorized();

            if (!int.TryParse(sub, out var userId))
                return BadRequest(new { error = "Invalid user id claim." });

            
            var user = await _users.GetByIdAsync(userId);
            if (user is null)
                return NotFound(new { error = "User not found." });

            
            return Ok(
                new
                {
                    UserId = int.Parse(sub),
                    email = user.Email,
                    username = user.Username,
                    name = user.Username, 
                    profilePictureUrl = user.ProfilePictureUrl,
                }
            );
        }

        
        
        
        [Authorize]
        [HttpPut("me")]
        public async Task<ActionResult<UserResponseDto>> EditProfile(
            [FromBody] EditProfileRequest req,
            CancellationToken ct
        )
        {
            var sub =
                User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub is null)
                return Unauthorized();

            var cmd = new EditProfileCommand
            {
                UserId = int.Parse(sub),
                Email = req.Email,
                Username = req.Username,
                ProfilePictureUrl = req.ProfilePictureUrl,
            };

            try
            {
                var dto = await _mediator.Send(cmd, ct);
                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        
        
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id, CancellationToken ct)
        {
            var command = new DeleteUserCommand { UserId = id };

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
                Console.WriteLine($"Error deleting user {id}: {ex}");
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }





}

