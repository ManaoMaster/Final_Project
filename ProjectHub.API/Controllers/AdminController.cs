using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Users;
using ProjectHub.Application.Features.Users.AdminUpdateUser;
using ProjectHub.Application.Features.Users.Queries.GetAllUsers;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]      // => /api/Admin
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: /api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            return Ok(users);
        }

        // PUT: /api/Admin/users/{id}
        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            [FromBody] AdminUpdateUserRequest request,
            CancellationToken ct)
        {
            var cmd = new AdminUpdateUserCommand
            {
                UserId = id,
                Email = request.Email,
                Username = request.Username,
                Role = request.Role,
                ProfilePictureUrl = request.ProfilePictureUrl
            };

            var dto = await _mediator.Send(cmd, ct);
            return Ok(dto);
        }
    }
}
