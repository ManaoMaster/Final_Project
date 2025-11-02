namespace ProjectHub.API.Contracts.Relationships
{
    // Request DTO สำหรับ Update
    // เราจะรับแค่ Metadata
    public record UpdateRelationshipRequest(string? DisplayName, string? Notes);
}
