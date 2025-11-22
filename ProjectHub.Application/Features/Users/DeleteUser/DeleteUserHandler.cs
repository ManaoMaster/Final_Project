using System; 
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Users.DeleteUser
{
    
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken
        )
        {
            
            
            var userExists = await _userRepository.GetUserByIdAsync(request.UserId);
            if (userExists == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} not found.");
                
            }

            
            await _userRepository.DeleteUserAsync(request.UserId);

            
            return Unit.Value;
        }
    }
}