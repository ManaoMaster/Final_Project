using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;




namespace ProjectHub.Application.Features.Relationships.UpdateRelationship
{
    public class UpdateRelationshipHandler : IRequestHandler<UpdateRelationshipCommand>
    {
        private readonly IRelationshipRepository _relationshipRepository;

        public UpdateRelationshipHandler(IRelationshipRepository relationshipRepository)
        {
            _relationshipRepository = relationshipRepository;
        }

        public async Task Handle(
            UpdateRelationshipCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var relationshipToUpdate = await _relationshipRepository.GetByIdAsync(request.Id);

            
            if (relationshipToUpdate == null)
            {
                
                
                throw new Exception($"Relationship with ID {request.Id} not found.");
            }

            
            relationshipToUpdate.DisplayName = request.DisplayName;
            relationshipToUpdate.Notes = request.Notes;

            
            await _relationshipRepository.UpdateAsync(relationshipToUpdate);
        }
    }
}
