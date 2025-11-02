using System.ComponentModel.DataAnnotations;

namespace ProjectHub.API.Contracts.Relationships
{
    public record CreateRelationshipRequest(
        string? DisplayName,
        string? Notes,
        int PrimaryTableId,
        int PrimaryColumnId,
        int ForeignTableId,
        int ForeignColumnId
    );
}
