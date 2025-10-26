using System;

namespace ProjectHub.Application.DTOs
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
    }
}

