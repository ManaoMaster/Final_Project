using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; // ใช้ Entity Tables
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Tables.CreateTable
{
    // Handler รับ CreateTableCommand และคืนค่า TableResponseDto
    public class CreateTableHandler : IRequestHandler<CreateTableCommand, TableResponseDto>
    {
        // Inject Dependencies ที่ต้องการ
        private readonly ITableRepository _tableRepository;
        private readonly IProjectRepository _projectRepository; // เพิ่มเข้ามาเพื่อเช็ค Project
        private readonly IMapper _mapper;

        // Constructor Injection
        public CreateTableHandler(
            ITableRepository tableRepository, 
            IProjectRepository projectRepository, // เพิ่ม Project Repository
            IMapper mapper)
        {
            _tableRepository = tableRepository;
            _projectRepository = projectRepository; // เก็บค่า
            _mapper = mapper;
        }

        public async Task<TableResponseDto> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            // --- Business Logic ---
            // 1. ตรวจสอบว่า ProjectId มีอยู่จริง (Optional แต่แนะนำ)
            var projectExists = await _projectRepository.GetProjectByIdAsync(request.ProjectId); 
             if (projectExists == null)
             {
                 throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
             }

            // 2. ตรวจสอบ Input (เช่น ชื่อห้ามว่าง)
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Table name is required.");
            }

            // 3. ตรวจสอบชื่อตารางซ้ำ (เฉพาะใน Project นี้)
            var isDuplicate = await _tableRepository.IsTableNameUniqueForProjectAsync(request.ProjectId, request.Name);
            if (isDuplicate)
            {
                throw new ArgumentException($"Table name '{request.Name}' is already in use in this project.");
            }

            // --- Mapping & Persistence ---
            // 4. ใช้ AutoMapper แปลง Command -> Entity Tables
            // *** แก้ไข: ใช้ชื่อเต็มเพื่อแก้ Namespace Collision ***
            var tableEntity = _mapper.Map<ProjectHub.Domain.Entities.Tables>(request); 
            //    (Mapper จะ Ignore Table_id, Created_at ตาม Profile)
            //    (Mapper จะ Map ProjectId -> Project_id, Name -> Name)

            // 5. เรียก Repository เพื่อบันทึก Entity ลง Database
            await _tableRepository.AddTableAsync(tableEntity);
            //    (Repository Implementation ใน Infrastructure จะจัดการ SaveChanges)

            // --- Response Mapping ---
            // 6. ใช้ AutoMapper แปลง Entity ที่บันทึกแล้ว -> Response DTO
            var responseDto = _mapper.Map<TableResponseDto>(tableEntity);

            return responseDto;
        }
    }
}