using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces; // (สำหรับ IProjectSecurityService)
using ProjectHub.Application.Repositories; // (สำหรับ ITableRepository)
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Features.Tables.GetAllTablesByProjectId;

namespace ProjectHub.Application.Features.Tables.GetAllTableByProjectId
{
    public class GetAllTablesByProjectIdHandler : IRequestHandler<GetAllTablesByProjectIdQuery, IEnumerable<TableResponseDto>>
    {
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;
        private readonly IProjectSecurityService _securityService;

        public GetAllTablesByProjectIdHandler(ITableRepository tableRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<IEnumerable<TableResponseDto>> Handle(GetAllTablesByProjectIdQuery request, CancellationToken cancellationToken)
        {
            // 1. (Security) ตรวจสอบสิทธิ์ก่อนว่า User คนนี้ มีสิทธิ์ดู Project นี้หรือไม่
            await _securityService.ValidateProjectAccessAsync(request.ProjectId); // (ผมเดาว่าคุณมี Method นี้)

            // 2. ดึงข้อมูลจาก Repository
            var tables = await _tableRepository.GetTablesByProjectIdAsync(request.ProjectId);

            // 3. Map Entity (Tables) -> DTO (TableResponseDto)
            return _mapper.Map<IEnumerable<TableResponseDto>>(tables);
        }
    }
}