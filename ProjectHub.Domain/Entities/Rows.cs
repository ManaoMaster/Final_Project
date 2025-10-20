using System.Text.Json.Serialization;

namespace ProjectHub.Domain.Entities
{
    public class Rows
    {
        public int Row_id { get; set; }
        public int Table_id { get; set; }
        public string Data { get; set; } = string.Empty;
        public DateTime Created_at { get; set; } = DateTime.UtcNow;

        public Tables? Table { get; set; }
    }


}