using AutoMapper;
using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Repositories;
using UserEntity = ProjectHub.Domain.Entities.Users;


namespace ProjectHub.Application.Features.Users.Register
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUserHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // validate เบื้องต้น
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Username)) throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.");

            // ตรวจ email ซ้ำ (ชื่อเมธอดปัจจุบัน 'IsEmailUniqueAsync' แต่คืน true เมื่อมีอยู่แล้ว)
            var exists = await _userRepository.IsEmailUniqueAsync(request.Email);
            if (exists)
                throw new ArgumentException("Email already exists.");

            // map Command -> Entity
            var user = _mapper.Map<UserEntity>(request);
            // (โปรด Hash Password จริง ๆ ในโปรดักชัน)
            user.Created_at = DateTime.UtcNow;

            await _userRepository.AddUserAsync(user);

            // ✅ map Entity -> DTO
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}
