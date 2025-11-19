using System;

namespace ProjectHub.Application.Dtos
{
    // DTO สำหรับข้อมูล Column ที่จะส่งกลับ
    public class ColumnResponseDto
    {
        public int ColumnId { get; set; }
        public int TableId { get; set; }
        public string Name { get; set; } = string.Empty; // กำหนด default
        public string DataType { get; set; } = string.Empty; // กำหนด default
        public bool IsPrimary { get; set; }
        public bool IsNullable { get; set; }

        // ใช้บอกว่า PK เป็น AUTO_INCREMENT หรือ MANUAL
        public string? PrimaryKeyType { get; set; }

        // ---------- LOOKUP meta ----------
        public int? LookupRelationshipId { get; set; }
        public int? LookupTargetColumnId { get; set; }

        // ถ้าอยากรู้ table และ column ปลายทางสำหรับทำ dropdown สวย ๆ
        public int? LookupTargetTableId { get; set; }
        public string? LookupTargetColumnName { get; set; }

    
        public string? FormulaDefinition { get; set; }
    }
}

