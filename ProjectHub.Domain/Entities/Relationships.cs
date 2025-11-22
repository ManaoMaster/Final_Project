using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHub.Domain.Entities
{
    
    public class Relationships
    {
        [Key]
        public int RelationshipId { get; set; }

        
        public int PrimaryTableId { get; set; } 
        public int PrimaryColumnId { get; set; } 

        
        public int ForeignTableId { get; set; } 
        public int? ForeignColumnId { get; set; } 

        
        
        public string? DisplayName { get; set; } 
        public string? Notes { get; set; } 

        
        

        
        [ForeignKey("PrimaryTableId")] 
        public Tables? PrimaryTable { get; set; }

        
        [ForeignKey("PrimaryColumnId")] 
        public Columns? PrimaryColumn { get; set; }

        
        [ForeignKey("ForeignTableId")] 
        public Tables? ForeignTable { get; set; }

        
        [ForeignKey("ForeignColumnId")] 
        public Columns? ForeignColumn { get; set; }
    }
}