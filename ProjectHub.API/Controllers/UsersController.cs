using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Users;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Users.EditProfile;
using ProjectHub.Application.Features.Users.Login;
using ProjectHub.Application.Features.Users.Register;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UsersController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // ------------------------
        // Register
        // ------------------------
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(
            [FromBody] RegisterUserRequest request,
            CancellationToken ct)
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var userDto = await _mediator.Send(command, ct);
            return Created($"/api/users/{userDto.UserId}", userDto);
        }

        // ------------------------
        // Login  
        // ------------------------
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(
            [FromBody] LoginRequest req,
            CancellationToken ct)
        {
            try
            {
                var token = await _mediator.Send(
                    new LoginCommand { Email = req.Email, Password = req.Password }, ct);
                return Ok(token);
            }
            catch (ArgumentException)
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }
        }

        // ------------------------
        // ดูข้อมูลตัวเองจาก Token (ทดสอบ JWT)
        // ------------------------
        [Authorize]
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            return Ok(new
            {
                sub = User.FindFirst("sub")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst("email")?.Value
                        ?? User.FindFirst(ClaimTypes.Email)?.Value,
                name = User.Identity?.Name
            });
        }

        //edit profile
      

        [Authorize]
        [HttpPut("me")]
        public async Task<ActionResult<UserResponseDto>> EditProfile(
            [FromBody] EditProfileRequest req, CancellationToken ct)
        {
            var sub = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sub is null) return Unauthorized();

            var cmd = new EditProfileCommand
            {
                UserId = int.Parse(sub),
                Email = req.Email,
                Username = req.Username
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
    }
}
