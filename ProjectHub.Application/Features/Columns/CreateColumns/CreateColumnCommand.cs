using MediatR;
using ProjectHub.Application.Dtos;
using System.ComponentModel.DataAnnotations; // ต้องใช้ [Required]

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnCommand : IRequest<ColumnResponseDto>
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        [MaxLength(100)] // เพิ่ม Validation เบื้องต้น
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)] // เพิ่ม Validation เบื้องต้น
        public string DataType { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false; // Default เป็น false

        public bool IsNullable { get; set; } = true; // Default เป็น true

        public string? FormulaDefinition { get; set; }
    }
}
