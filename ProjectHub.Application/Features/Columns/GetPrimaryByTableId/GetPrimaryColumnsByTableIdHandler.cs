using MediatR;
using ProjectHub.Application.Repositories; // <-- ตรวจสอบ Using (สำหรับ IColumnRepository)
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId
{
    public class GetPrimaryColumnsByTableIdQueryHandler : IRequestHandler<GetPrimaryColumnsByTableIdQuery, IEnumerable<ColumnEntity>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService;

        public GetPrimaryColumnsByTableIdQueryHandler(IColumnRepository columnRepository, IProjectSecurityService securityService)
        {
            _columnRepository = columnRepository;
            _securityService = securityService;
        }

        public async Task<IEnumerable<ColumnEntity>> Handle(GetPrimaryColumnsByTableIdQuery request, CancellationToken cancellationToken)
        {
            await _securityService.ValidateTableAccessAsync(request.TableId);
            // 1. ดึงคอลัมน์ทั้งหมดของตารางนั้น
            var allColumns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);

            // 2. กรองเฉพาะคอลัมน์ที่ปักธง Is_primary = true เท่านั้น!
            // นี่คือส่วนที่ "สถาปนิก" ต้องใช้
            var primaryColumns = allColumns.Where(c => c.Is_primary);

            // 3. ส่งคืน (Dropdown ของ UI ก็จะแสดงแค่รายการนี้)
            return primaryColumns;
        }
    }
}