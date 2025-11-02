using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnCommand : IRequest<ColumnResponseDto>
    {
        // (Property พื้นฐานที่ Handler เรียกใช้)
        public int TableId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }

        // (Property ที่จำเป็นสำหรับ Logic ของคุณ)
        public bool IsNullable { get; set; } // (Entity Columns.cs มี )

        // --- *** นี่คือ Property ที่ขาดไป (และทำให้เกิด Error) *** ---
        public string? FormulaDefinition { get; set; }
    }
}