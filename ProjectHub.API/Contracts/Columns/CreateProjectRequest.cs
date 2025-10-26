namespace ProjectHub.API.Contracts.Columns
{
    public record CreateColumnRequest(int TableId, string Name,string DataType, bool IsNullable, bool IsPrimaryKey);
}
