using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    
    
    public class NewRelationshipData
    {
        public int PrimaryTableId { get; set; }
        public int PrimaryColumnId { get; set; }
        public int ForeignTableId { get; set; }
        public int? ForeignColumnId { get; set; }
    }

    public class CreateColumnCommand : IRequest<ColumnResponseDto>
    {
        
        public int TableId { get; set; }
        public required string Name { get; set; }
        public required string DataType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsNullable { get; set; } = true;
        public string? PrimaryKeyType { get; set; }
        public string? FormulaDefinition { get; set; }
        public int? LookupTargetColumnId { get; set; }

        

        
        public int? LookupRelationshipId { get; set; }

        
        public NewRelationshipData? NewRelationship { get; set; }
    }
}