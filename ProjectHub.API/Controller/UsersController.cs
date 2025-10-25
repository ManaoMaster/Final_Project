// ใน ProjectHub.API/Controllers/UsersController.cs

using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Users.Register;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public record RegisterUserRequest(string Email, string Username, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
            };

            try
            {
                // 1. userResponse จะเป็น DTO ที่ได้รับกลับมาจาก Handler
                UserResponseDto userResponse = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(Register),
                    // 2. Route value คือ id ของ User
                    new { id = userResponse.UserId },
                    // 3. Body ของ Response คือ DTO ทั้งหมด (ไม่มี Password)
                    userResponse
                );
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
