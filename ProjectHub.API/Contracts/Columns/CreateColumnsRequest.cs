namespace ProjectHub.API.Contracts.Columns
{
    // *** [ADD] 1. สร้าง Record ใหม่สำหรับ "ไก่" (ข้อมูล Relation ใหม่) ***
    // (นี่คือ JSON object ที่ Swagger/API จะเห็น)
    public record NewRelationshipDataRequest(
        int PrimaryTableId,
        int PrimaryColumnId,
        int ForeignTableId,
        int? ForeignColumnId
    );

    // *** [UPGRADE] 2. อัปเกรด Request หลัก ***
    public record CreateColumnRequest(
        int TableId,
        string Name,
        string DataType,
        bool IsNullable,
        bool IsPrimary,
        string? FormulaDefinition,
        int? LookupTargetColumnId,

        // --- ส่วนที่แก้ปัญหาไก่กับไข่ ---
        
        // "ไข่" (ส่ง ID เดิม)
        int? LookupRelationshipId, 
        
        // "ไก่" (ส่งข้อมูลใหม่)
        NewRelationshipDataRequest? NewRelationship 
    );
}