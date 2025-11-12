using ProjectHub.Application.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt; // (ตั้งชื่อเล่นให้สั้นลง)

namespace ProjectHub.Infrastructure.Services
{
    // นี่คือ "ตัวทำงานจริง" ที่ Implement "สัญญา" (IPasswordHasher)
    public class PasswordHasher : IPasswordHasher
    {
        // 1. Implement Method "สร้าง" Hash
        public string Hash(string password)
        {
            // เราใช้ BCrypt (WorkFactor = 12) เพื่อสร้าง Hash ที่ปลอดภัย
            return BCryptNet.HashPassword(password, 12);
        }

        // 2. Implement Method "ตรวจสอบ" Hash
        public bool Verify(string passwordHash, string password)
        {
            // BCrypt จะเปรียบเทียบรหัสที่ User พิมพ์ (password)
            // กับ Hash ที่เก็บใน DB (passwordHash) ให้เอง
            return BCryptNet.Verify(password, passwordHash);
        }
    }
}