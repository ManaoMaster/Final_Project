using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.API.Contracts.Users;
using ProjectHub.Application.DTOs;
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

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var userDto = await _mediator.Send(command, ct);

            
            return Created($"/api/users/{userDto.UserId}", userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var token = await _mediator.Send(new LoginCommand { Email = req.Email, Password = req.Password }, ct);
            return Ok(token);
        }
    }
}
