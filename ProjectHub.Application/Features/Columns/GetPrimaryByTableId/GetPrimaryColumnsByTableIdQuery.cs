using MediatR;
using ColumnEntity = ProjectHub.Domain.Entities.Columns; // <-- [FIX 1] ใช้ Alias
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId
{
    // Query นี้ส่งไปเพื่อ "ขอ" คอลัมน์ที่เป็น Primary Key
    public class GetPrimaryColumnsByTableIdQuery : IRequest<IEnumerable<ColumnEntity>> // <-- [FIX 2]
    {
        public int TableId { get; set; }
    }
}