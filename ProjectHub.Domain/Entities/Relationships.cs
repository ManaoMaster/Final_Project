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
        // นี่คือ "ขาเข้า" (Primary) -> ID ของตารางหลัก (เช่น Products)
        public int PrimaryTableId { get; set; } // FK ไปยัง Tables

        // นี่คือ "ขาเข้า" (Primary) -> ID ของคอลัมน์หลัก (เช่น ProductId)
        public int PrimaryColumnId { get; set; } // FK ไปยัง Columns

        // --- Foreign Key (ปลายทาง) ---
        // นี่คือ "ขาออก" (Foreign) -> ID ของตารางลูก (เช่น Sales)
        public int ForeignTableId { get; set; } // FK ไปยัง Tables

        // นี่คือ "ขาออก" (Foreign) -> ID ของคอลัมน์ลูก (เช่น ProductId ใน Sales)
        public int ForeignColumnId { get; set; } // FK ไปยัง Columns

        // --- Navigation Properties ---
        // (EF Core ใช้สิ่งนี้เพื่อสร้างความสัมพันธ์)
        //

        // *** แก้ไข: ***
        // เชื่อมโยง Navigation 'PrimaryTable' กับ FK Property 'PrimaryTableId' (ใน Class นี้)
        [ForeignKey("PrimaryTableId")]
        public Tables? PrimaryTable { get; set; }

        // *** แก้ไข: ***
        // เชื่อมโยง Navigation 'PrimaryColumn' กับ FK Property 'PrimaryColumnId' (ใน Class นี้)
        [ForeignKey("PrimaryColumnId")]
        public Columns? PrimaryColumn { get; set; }

        // *** แก้ไข: ***
        // เชื่อมโยง Navigation 'ForeignTable' กับ FK Property 'ForeignTableId' (ใน Class นี้)
        [ForeignKey("ForeignTableId")]
        public Tables? ForeignTable { get; set; }

        // *** แก้ไข: ***
        // เชื่อมโยง Navigation 'ForeignColumn' กับ FK Property 'ForeignColumnId' (ใน Class นี้)
        [ForeignKey("ForeignColumnId")]
        public Columns? ForeignColumn { get; set; }
    }
}
