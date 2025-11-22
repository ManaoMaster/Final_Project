using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace ProjectHub.Domain.Entities
{
    public class Projects
    {
        [Key]
        public int Project_id { get; set; }

        public int User_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;

        

        
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
        
        public bool IsFavorite { get; set; } = false;

        
        
        public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;

        

        [ForeignKey("User_id")]
        public Users? Users { get; set; }

        public ICollection<Tables> Tables { get; set; } = new List<Tables>();
    }
}