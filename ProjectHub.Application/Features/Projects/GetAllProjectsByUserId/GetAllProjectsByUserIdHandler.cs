using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories; 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// *** [FIX 1] ***
// ลบ using ที่เกี่ยวกับ Http ออกให้หมด (ถ้ามี)
// using Microsoft.AspNetCore.Http; 
// using System.Security.Claims;

namespace ProjectHub.Application.Features.Projects.GetAllProjects
{
    public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, IEnumerable<ProjectResponseDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        // *** [FIX 2] ลบ IHttpContextAccessor ออก ***

        public GetAllProjectsHandler(IProjectRepository projectRepository, IMapper mapper) // <-- *** [FIX 3] ***
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectResponseDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            // 1. (Security) รับ UserId ที่ "สะอาด" มาจาก Query
            // (Controller เป็นคนรับผิดชอบในการหา UserId มาให้)
            var userId = request.UserId; 

            // 2. ดึงข้อมูลจาก Repository
            var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);

            // 3. Map Entity -> DTO
            return _mapper.Map<IEnumerable<ProjectResponseDto>>(projects);
        }
        
        // *** [FIX 4] ลบ Method GetCurrentUserId() ทิ้ง ***
    }
}