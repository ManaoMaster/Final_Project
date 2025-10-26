using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;



namespace ProjectHub.Application.Features.Users.EditProfile
{
    public class EditProfileHandler : IRequestHandler<EditProfileCommand, UserResponseDto>
    {
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;

        public EditProfileHandler(IUserRepository users, IMapper mapper)
        {
            _users = users;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Handle(EditProfileCommand request, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(request.UserId)
                       ?? throw new ArgumentException("User not found.");

            var newEmail = request.Email.Trim().ToLowerInvariant();
            var newUsername = request.Username.Trim();

            if (await _users.IsEmailUsedByOtherAsync(request.UserId, newEmail))
                throw new ArgumentException("Email is already in use.");

            if (await _users.IsUsernameUsedByOtherAsync(request.UserId, newUsername))
                throw new ArgumentException("Username is already in use.");

            user.Email = newEmail;
            user.Username = newUsername;

            await _users.UpdateUserAsync(user);

            return _mapper.Map<UserResponseDto>(user);
        }
    }
}
