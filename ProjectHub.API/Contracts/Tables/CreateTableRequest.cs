namespace ProjectHub.API.Contracts.Tables
{
    public record CreateTableRequest(int ProjectId, string Name, bool UseAutoIncrement);
}
