//using MediatR;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using ProjectHub.Application.Interfaces; // <-- 1. [ADD] Import "ยาม"
//using ProjectHub.Application.Repositories; // (สำหรับ IColumnRepository) [cite: 68]
//using ProjectHub.Domain.Entities; // (สำหรับ Columns) [cite: 92]
//using ColumnEntity = ProjectHub.Domain.Entities.Columns;
//using ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId; // (Alias ถ้าคุณใช้)

//namespace ProjectHub.Application.Features.Columns.GetPrimaryByTableId
//{
//    // (Handler นี้ใช้ Query 'GetColumnsByTableIdQuery' (Source 184))
//    public class GetColumnsByTableIdQueryHandler : IRequestHandler<GetColumnsByTableIdQuery, IEnumerable<ColumnEntity>>
//    {
//        private readonly IColumnRepository _columnRepository;
//        private readonly IProjectSecurityService _securityService; // 2. [ADD] Inject "ยาม"

//        public GetColumnsByTableIdQueryHandler(
//            IColumnRepository columnRepository,
//            IProjectSecurityService securityService) // 3. [ADD] รับ "ยาม"
//        {
//            _columnRepository = columnRepository;
//            _securityService = securityService; // 4. [ADD] กำหนดค่า
//        }

//        public async Task<IEnumerable<ColumnEntity>> Handle(GetColumnsByTableIdQuery request, CancellationToken cancellationToken)
//        {
//            // 5. [ADD] "ยาม" ตรวจสอบสิทธิ์ก่อน!
//            await _securityService.ValidateTableAccessAsync(request.TableId);

//            // (ถ้าผ่าน) ทำงานเดิมต่อ
//            // (GetColumnsByTableIdAsync มาจาก IColumnRepository (Source 68))
//            var allColumns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId); 
//            return allColumns;
//        }
//    }
//}

// GetColumnsByTableIdQueryHandler.cs
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Columns.GetPrimaryByTableId
{
    public class GetColumnsByTableIdQueryHandler
     : IRequestHandler<GetColumnsByTableIdQuery, IEnumerable<ColumnResponseDto>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService;
        private readonly IMapper _mapper;

        public GetColumnsByTableIdQueryHandler(
            IColumnRepository columnRepository,
            IProjectSecurityService securityService,
            IMapper mapper)
        {
            _columnRepository = columnRepository;
            _securityService = securityService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ColumnResponseDto>> Handle(
            GetColumnsByTableIdQuery request,
            CancellationToken cancellationToken)
        {
            await _securityService.ValidateTableAccessAsync(request.TableId);

            var allColumns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);

            return _mapper.Map<IEnumerable<ColumnResponseDto>>(allColumns);
        }
    }
}
