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

        public ICollection<Projects> Projects { get; set; } = new List<Projects>();
    }

    
}