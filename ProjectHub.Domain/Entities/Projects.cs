using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- เพิ่ม Using นี้

namespace ProjectHub.Domain.Entities
{
    public class Projects
    {
        [Key]
        public int Project_id { get; set; }

        public int User_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;

        // --- [FIX] เพิ่ม 3 Property ใหม่ ---

        // 1. สำหรับ "Last modified" (อัปเดตล่าสุด)
        // (เราจะตั้งค่าเริ่มต้นให้เป็นเวลาเดียวกับที่สร้าง)
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 2. สำหรับ "Favorite" (ดาว)
        // (ค่าเริ่มต้นคือ false)
        public bool IsFavorite { get; set; } = false;

        // 3. สำหรับ "Recent open" (เปิดล่าสุด)
        // (เราจะตั้งค่าเริ่มต้นให้เป็นเวลาเดียวกับที่สร้าง)
        public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;

        // --- [End of FIX] ---

        [ForeignKey("User_id")]
        public Users? Users { get; set; }

        public ICollection<Tables> Tables { get; set; } = new List<Tables>();
    }
}