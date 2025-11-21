using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Users.GetAllUsers;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper; // ถ้าใช้ AutoMapper

        public GetAllUsersHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken
        )
        {
            // 1. ต้องไปเพิ่ม Method GetAllAsync() ใน IUserRepository ก่อนนะ!
            var users = await _userRepository.GetAllAsync();

            // 2. แปลง Entity เป็น DTO
            return _mapper.Map<UserResponseDto>(users);
        }
    }
}
