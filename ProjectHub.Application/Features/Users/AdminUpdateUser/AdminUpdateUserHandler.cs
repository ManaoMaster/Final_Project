using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Features.Users.AdminUpdateUser
{
    public class AdminUpdateUserHandler : IRequestHandler<AdminUpdateUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;

        public AdminUpdateUserHandler(IUserRepository users, IMapper mapper)
        {
            _users = users;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Handle(AdminUpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException($"User id {request.UserId} not found.");
            }

            // update field ตาม request
            user.Email = request.Email.Trim();
            user.Username = request.Username.Trim();
            if (!string.IsNullOrWhiteSpace(request.Role))
                user.Role = request.Role.Trim();
            if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
                user.ProfilePictureUrl = request.ProfilePictureUrl;

            await _users.UpdateAsync(user);   

            return _mapper.Map<UserResponseDto>(user);
        }
    }

}
