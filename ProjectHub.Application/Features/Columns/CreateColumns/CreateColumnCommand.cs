using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    // *** [FIX 1] ***
    // สร้าง Class ใหม่เพื่อเก็บข้อมูล Relationship
    public class NewRelationshipData
    {
        public int PrimaryTableId { get; set; }
        public int PrimaryColumnId { get; set; }
        public int ForeignTableId { get; set; }
        public int ForeignColumnId { get; set; }
    }

    public class CreateColumnCommand : IRequest<ColumnResponseDto>
    {
        // --- ข้อมูล Column เดิม ---
        public int TableId { get; set; }
        public required string Name { get; set; }
        public required string DataType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsNullable { get; set; } = true;
        public string? PrimaryKeyType { get; set; }
        public string? FormulaDefinition { get; set; }
        public int? LookupTargetColumnId { get; set; }

        // --- [FIX 2] แก้ปัญหาไก่กับไข่ ---

        // 1. "ไข่" (ถ้ามี ID อยู่แล้ว)
        public int? LookupRelationshipId { get; set; }

        // 2. "ไก่" (ข้อมูลสำหรับสร้าง Relationship ใหม่ ถ้า LookupRelationshipId เป็น null)
        public NewRelationshipData? NewRelationship { get; set; }
    }
}