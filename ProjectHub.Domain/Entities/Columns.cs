using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHub.Domain.Entities
{
    public class Columns
    {
        [Key]
        public int Column_id { get; set; }
        public int Table_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Data_type { get; set; } = string.Empty;
        public Boolean Is_primary { get; set; }
        public Boolean Is_nullable { get; set; }

        [ForeignKey("Table_id")] 
        public Tables? Tables { get; set; }
    }
}
