namespace ProjectHub.API.Contracts.Columns
{
    public record UpdateColumnRequest(
        string ColumnName,
        string DataType,
        bool IsPrimary,
        bool IsNullable
    );
}
