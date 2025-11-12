using MediatR;

namespace ProjectHub.Application.Features.Users.ChangePassword
{
    // นี่คือ "คำสั่ง" สำหรับเปลี่ยนรหัสผ่าน
    // IRequest<Unit> หมายความว่า "คำสั่งนี้เมื่อทำเสร็จ ไม่ต้องคืนค่าอะไรกลับไป" (จะกลายเป็น 204 No Content)
    public class ChangePasswordCommand : IRequest<Unit>
    {
        // เราจะให้ Controller "ยัด" UserId ที่ล็อกอินอยู่เข้ามาในนี้ (เพื่อความปลอดภัย)
        public int UserId { get; set; }

        // ส่วนนี้ User ต้องกรอกมาจากหน้าบ้าน
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}