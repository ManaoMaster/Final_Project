using System;

namespace ProjectHub.Application.Dtos
{
    
    public class ColumnResponseDto
    {
        public int ColumnId { get; set; }
        public int TableId { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string DataType { get; set; } = string.Empty; 
        public bool IsPrimary { get; set; }
        public bool IsNullable { get; set; }

        
        public string? PrimaryKeyType { get; set; }

        
        public int? LookupRelationshipId { get; set; }
        public int? LookupTargetColumnId { get; set; }

        
        public int? LookupTargetTableId { get; set; }
        public string? LookupTargetColumnName { get; set; }

    
        public string? FormulaDefinition { get; set; }
    }
}

