using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Users;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.DeleteProject;
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
            CancellationToken ct
        )
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

        // ------------------------
        // ดูข้อมูลตัวเองจาก Token (ทดสอบ JWT)
        // ------------------------
        [Authorize]
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            return Ok(
                new
                {
                    sub = User.FindFirst("sub")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    email = User.FindFirst("email")?.Value
                        ?? User.FindFirst(ClaimTypes.Email)?.Value,
                    name = User.Identity?.Name,
                }
            );
        }

        //edit profile

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
                ProfilePictureUrl = req.ProfilePictureUrl   
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

        // --- Endpoint: DELETE /api/rows/{id} ---
        [HttpDelete("{id}")] // รับ ID จาก URL Path
        public async Task<IActionResult> DeleteUser(
            [FromRoute] int id, // ดึง ID จาก Path
            CancellationToken ct
        ) // เพิ่ม ct
        {
            // สร้าง Command โดยตรงจาก ID ที่ได้จาก Route
            var command = new DeleteUserCommand { UserId = id };

            try
            {
                // ส่ง Command ให้ MediatR (Handler จะคืน Unit)
                await _mediator.Send(command, ct); // เพิ่ม ct

                // คืน 204 No Content = สำเร็จ ไม่มีข้อมูลส่งกลับ
                return NoContent();
            }
            catch (ArgumentException ex) // จับ Error จาก Handler (Row not found)
            {
                return NotFound(new { Error = ex.Message }); // คืน 404 Not Found
            }
            catch (Exception ex) // จับ Error อื่นๆ
            {
                Console.WriteLine($"Error deleting user {id}: {ex}"); // Log ง่ายๆ
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
