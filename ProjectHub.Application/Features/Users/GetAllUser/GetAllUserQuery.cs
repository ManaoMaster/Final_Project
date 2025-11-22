using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Users.Queries.GetAllUsers
{
    
    public class GetAllUsersQuery : IRequest<List<UserResponseDto>>
    {
    }
}
