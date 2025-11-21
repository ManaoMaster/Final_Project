using MediatR;
using ProjectHub.Application.Dtos; // หรือที่เก็บ UserDto ของคุณ

namespace ProjectHub.Application.Features.Users.GetAllUsers
{
    // Request: ไม่ต้องส่งอะไรมา เพราะจะเอาทั้งหมด
    // Response: List ของ UserDto
    public class GetAllUsersQuery : IRequest<UserResponseDto> { }
}
