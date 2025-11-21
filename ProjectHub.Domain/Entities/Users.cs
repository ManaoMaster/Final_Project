using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Domain.Entities
{
    public class Users
    {
        [Key]
        public int User_id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;

        public string Role { get; set; } = UserRoles.User;

        public ICollection<Projects> Projects { get; set; } = new List<Projects>();

        public string ProfilePictureUrl { get; set; } = string.Empty;
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
