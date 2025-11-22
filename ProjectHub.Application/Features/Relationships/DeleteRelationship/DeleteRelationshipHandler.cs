using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Relationships.DeleteRelationship
{
    public class DeleteRelationshipHandler : IRequestHandler<DeleteRelationshipCommand>
    {
        private readonly IRelationshipRepository _relationshipRepository;

        public DeleteRelationshipHandler(IRelationshipRepository relationshipRepository)
        {
            _relationshipRepository = relationshipRepository;
        }

        public async Task Handle(DeleteRelationshipCommand request, CancellationToken cancellationToken)
        {
            
            var relationshipToDelete = await _relationshipRepository.GetByIdAsync(request.Id);

            
            if (relationshipToDelete == null)
            {
                throw new Exception($"Relationship with ID {request.Id} not found.");
            }

            
            await _relationshipRepository.DeleteAsync(relationshipToDelete);
        }
    }
}