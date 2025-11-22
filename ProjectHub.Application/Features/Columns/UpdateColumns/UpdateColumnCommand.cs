using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos; 
using System.Text.Json.Serialization;


namespace ProjectHub.Application.Features.Columns.UpdateColumn
{
    
    public class UpdateColumnCommand : IRequest<ColumnResponseDto>
    {
        [Required]
        public int ColumnId { get; set; } 

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("columnName")]
        public string NewName { get; set; } = string.Empty; 

        [Required]
        [JsonPropertyName("dataType")]
        public string NewDataType { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("isPrimary")]
        public bool NewIsPrimary { get; set; }

        [Required]
        [JsonPropertyName("isNullable")]
        public bool NewIsNullable { get; set; }
    }
}
