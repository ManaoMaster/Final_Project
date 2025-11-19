using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHub.Domain.Entities
{
    // ตารางนี้ใช้เก็บ Metadata (กฎ) ว่า Table/Column ไหน เชื่อมกับ Table/Column ไหน
    public class Relationships
    {
        [Key]
        public int RelationshipId { get; set; }

        // --- Foreign Key (ต้นทาง) ---
        public int PrimaryTableId { get; set; } // FK ไปยัง Tables
        public int PrimaryColumnId { get; set; } // FK ไปยัง Columns

        // --- Foreign Key (ปลายทาง) ---
        public int ForeignTableId { get; set; } // FK ไปยัง Tables
        public int? ForeignColumnId { get; set; } // FK ไปยัง Columns

        // --- *** METADATA ที่เราเพิ่มเข้ามา *** ---
        // (เราจะใช้สิ่งนี้ใน "Update")
        public string? DisplayName { get; set; } // เช่น "ผู้เขียนบทความ", "สินค้าในออเดอร์"
        public string? Notes { get; set; } // หมายเหตุที่ User พิมพ์ไว้

        // --- Navigation Properties ---
        // (EF Core ใช้สิ่งนี้เพื่อสร้างความสัมพันธ์)

        // เชื่อมโยง PrimaryTableId กับตาราง Tables
        [ForeignKey("PrimaryTableId")] // <-- *แก้ไข* ผมเปลี่ยนจาก TableId เป็นชื่อที่ถูกต้อง
        public Tables? PrimaryTable { get; set; }

        // เชื่อมโยง PrimaryColumnId กับตาราง Columns
        [ForeignKey("PrimaryColumnId")] // <-- *แก้ไข* ผมเปลี่ยนจาก ColumnId เป็นชื่อที่ถูกต้อง
        public Columns? PrimaryColumn { get; set; }

        // เชื่อมโยง ForeignTableId กับตาราง Tables
        [ForeignKey("ForeignTableId")] // <-- *แก้ไข* ผมเปลี่ยนจาก TableId เป็นชื่อที่ถูกต้อง
        public Tables? ForeignTable { get; set; }

        // เชื่อมโยง ForeignColumnId กับตาราง Columns
        [ForeignKey("ForeignColumnId")] // <-- *แก้ไข* ผมเปลี่ยนจาก ColumnId เป็นชื่อที่ถูกต้อง
        public Columns? ForeignColumn { get; set; }
    }
}