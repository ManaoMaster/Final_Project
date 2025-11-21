namespace ProjectHub.API.Contracts.Users
{
    public class AdminUpdateUserRequest
    {

        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        // ให้ Admin ปรับ role ได้ด้วย (เช่น "Admin", "User")
        public string? Role { get; set; }

        // เผื่ออนาคตอยากแก้รูปโปรไฟล์จากหน้านี้
        public string? ProfilePictureUrl { get; set; }
    }
}
