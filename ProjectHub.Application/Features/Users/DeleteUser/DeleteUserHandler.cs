using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Users.DeleteUser
{
    // Handler for DeleteUserCommand, returns Unit (nothing)
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
            // Optional but recommended: Check if the user exists before deleting
            // This provides a clearer error message than letting the repository handle it silently
            var userExists = await _userRepository.GetUserByIdAsync(request.UserId);
            if (userExists == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} not found.");
                // Or use a custom NotFoundException
            }

            // Call the repository to delete the user by ID
            await _userRepository.DeleteUserAsync(request.UserId);

            // Return Unit.Value to indicate success with no return data
            return Unit.Value;
        }
    }
}