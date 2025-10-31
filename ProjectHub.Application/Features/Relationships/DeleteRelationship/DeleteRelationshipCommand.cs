using MediatR;

namespace ProjectHub.Application.Features.Relationships.DeleteRelationship
{
    public class DeleteRelationshipCommand : IRequest
    {
        public int Id { get; set; }
    }
}