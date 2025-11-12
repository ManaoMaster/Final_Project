using System;

namespace ProjectHub.Application.Dtos
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- [FIX] เพิ่ม 3 Property ใหม่ ---
        // (เพื่อให้ตรงกับ Mockup ของคุณ)

        // 1. วันที่แก้ไขล่าสุด (Last modified)
        public DateTime UpdatedAt { get; set; }

        // 2. จำนวนโต๊ะ (15 tables)
        public int TableCount { get; set; }

        // 3. ติดดาว (Favorite)
        public bool IsFavorite { get; set; }
    }
}