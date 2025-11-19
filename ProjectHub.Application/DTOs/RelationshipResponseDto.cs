using System;

namespace ProjectHub.Application.DTOs
{
    // DTO สำหรับส่งข้อมูล "กฎ" ความสัมพันธ์กลับไป
    public class RelationshipResponseDto
    {
        public int RelationshipId { get; set; }
        public int PrimaryTableId { get; set; }
        public int PrimaryColumnId { get; set; }
        public int ForeignTableId { get; set; }
        public int? ForeignColumnId { get; set; }
    }
}
