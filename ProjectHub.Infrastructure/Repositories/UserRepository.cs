using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence; // 2. อ้างอิง AppDbContext

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

        public async Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Implement การ validate หา id ก่อน new project
        public Task<bool> ExistsAsync(int userId) =>
            _context.Users.AnyAsync(u => u.User_id == userId);

        // ตรวจสอบ Email ใน DB เพื่อ check login
        public Task<Users?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<Users?> GetByIdAsync(int userId) =>
            _context.Users.FirstOrDefaultAsync(u => u.User_id == userId);

        public Task<bool> IsUsernameUsedByOtherAsync(int userId, string username) =>
            _context.Users.AnyAsync(u => u.User_id != userId && u.Username == username);

        public Task<bool> IsEmailUsedByOtherAsync(int userId, string email) =>
            _context.Users.AnyAsync(u =>
                u.User_id != userId && u.Email.ToLower() == email.ToLower()
            );

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            // ใช้ FindAsync ซึ่งเป็นวิธีที่เร็วที่สุดในการหา Entity ตาม Primary Key
            return await _context.Users.FindAsync(userId);
        }

        public async Task DeleteUserAsync(int userId)
        {
            // 1. ค้นหา Row ด้วย ID
            var userToDelete = await _context.Users.FindAsync(userId);

            // 2. ถ้าเจอ ให้สั่งลบ
            if (userToDelete != null)
            {
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();
                // ไม่จำเป็นต้องกังวลเรื่อง Cascade Delete ที่นี่ เพราะ Row ไม่มีข้อมูลลูก
            }
            // ถ้าไม่เจอ ก็ไม่ต้องทำอะไร (Handler ควรจะเช็คเจอไปก่อนแล้ว)
        }
    }
}
