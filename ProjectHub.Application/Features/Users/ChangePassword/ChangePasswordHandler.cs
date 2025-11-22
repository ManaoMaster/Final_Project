using MediatR;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Users.ChangePassword
{
    
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;

        
        public ChangePasswordHandler(IUserRepository userRepository, IPasswordHasher hasher)
        {
            _userRepository = userRepository;
            _hasher = hasher;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            
            
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found."); 
            }

            
            
            var isPasswordCorrect = _hasher.Verify(user.Password, request.CurrentPassword);

            
            if (!isPasswordCorrect)
            {
                throw new ArgumentException("Current password is incorrect.");
            }

            
            var newPasswordHash = _hasher.Hash(request.NewPassword);

            
            user.Password = newPasswordHash;

            
            
            await _userRepository.UpdateUserAsync(user);

            
            return Unit.Value;
        }
    }
}