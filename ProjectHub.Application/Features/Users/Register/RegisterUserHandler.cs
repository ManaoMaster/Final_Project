using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Features.Users.Register
{
    // 2. เปลี่ยน return type จาก int เป็น UserResponseDto
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // 3. เปลี่ยน Task<int> เป็น Task<UserResponseDto>
        public async Task<UserResponseDto> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var isDuplicate = await _userRepository.IsEmailUniqueAsync(request.Email);
            if (isDuplicate)
            {
                throw new Exception("Email already exists.");
            }

            // 4. (คงการแก้ไข Namespace Collision ไว้)
            var user = new ProjectHub.Domain.Entities.Users
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password, // (ในโปรเจกต์จริง ต้อง HASH PASSWORD ก่อน!)
                Created_at = DateTime.UtcNow,
            };

            await _userRepository.AddUserAsync(user);

            // 5. สร้าง DTO เพื่อส่งกลับ (ป้องกัน Password รั่วไหล)
            var responseDto = new UserResponseDto
            {
                UserId = user.User_id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.Created_at,
            };

            return responseDto; // 6. คืนค่า DTO
        }
    }
}
