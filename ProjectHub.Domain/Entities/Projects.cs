namespace ProjectHub.Domain.Entities
{
    public class Projects
    {
        public int Project_id {get; set;}
        public int User_id {get; set;}
        public string Name {get; set;} = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;
        
        public Users? Users { get; set; }
        public ICollection<Tables> Tables { get; set; } = new List<Tables>();

    }


}