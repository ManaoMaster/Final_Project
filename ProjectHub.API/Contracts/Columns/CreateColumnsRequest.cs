namespace ProjectHub.API.Contracts.Columns
{
    
    
    public record NewRelationshipDataRequest(
        int PrimaryTableId,
        int PrimaryColumnId,
        int ForeignTableId,
        int? ForeignColumnId
    );

    
    public record CreateColumnRequest(
        int TableId,
        string Name,
        string DataType,
        bool IsNullable,
        bool IsPrimary,
        string? FormulaDefinition,
        int? LookupTargetColumnId,

        
        
        
        int? LookupRelationshipId, 
        
        
        NewRelationshipDataRequest? NewRelationship 
    );
}