using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Repositories; // 1. อ้างอิง Interface (สัญญา) จาก Application
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence; // 2. อ้างอิง AppDbContext
using System.Threading.Tasks;

// 3. ใช้ Namespace ที่ตรงกับที่ Program.cs เรียกหา
namespace ProjectHub.Infrastructure.Repositories 
{
    // 4. สร้าง Class ที่ Implement Interface
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        // 5. ใช้ Dependency Injection (DI) เพื่อรับ AppDbContext
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // 6. Implement การทำงานของ Interface (AddUserAsync)
        public async Task AddUserAsync(Users user)
        {
            // ใช้ EF Core เพื่อเพิ่ม Entity
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // 7. Implement การทำงานของ Interface (IsEmailUniqueAsync)
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            // ใช้ EF Core เพื่อตรวจสอบข้อมูลในตาราง Users
            // จะคืนค่า True ถ้ามี Email นี้อยู่แล้ว (ซ้ำ)
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // 8. Implement การ validate หา id ก่อน new project
        public Task<bool> ExistsAsync(int userId)
    => _context.Users.AnyAsync(u => u.User_id == userId);
    }
}
