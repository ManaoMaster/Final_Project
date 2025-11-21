using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Users.Queries.GetAllUsers;

namespace ProjectHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]          // => /api/Admin
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers(CancellationToken ct)
        {
            // ส่ง Query ไปให้ Handler ทำงาน
            var users = await _mediator.Send(new GetAllUsersQuery(), ct);

            // users = List<UserResponseDto>
            return Ok(users);
        }
    }
}
