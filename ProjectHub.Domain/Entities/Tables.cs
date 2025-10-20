namespace ProjectHub.Domain.Entities
{
    public class Tables
    {
        public int Table_id { get; set; }
        public int Project_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;

        public Projects? Projects { get; set; }
        public ICollection<Columns> Columns { get; set; } = new List<Columns>();
        public ICollection<Rows> Rows { get; set; } = new List<Rows>();
    }


}