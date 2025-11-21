using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Users.Queries.GetAllUsers
{
    // เดิมน่าจะเป็น IRequest<UserResponseDto>
    public class GetAllUsersQuery : IRequest<List<UserResponseDto>>
    {
    }
}
