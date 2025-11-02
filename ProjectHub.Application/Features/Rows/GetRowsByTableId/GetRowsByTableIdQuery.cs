using MediatR;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    /// <summary>
    /// Query สำหรับดึงข้อมูลแถวทั้งหมดใน Table ที่ระบุ
    /// </summary>
    public class GetRowsByTableIdQuery : IRequest<IEnumerable<IDictionary<string, object>>>
    {
        /// <summary>
        /// ID ของตาราง (Tables.Table_id) ที่ต้องการดึงข้อมูล
        /// </summary>
        public int TableId { get; set; }
    }
}