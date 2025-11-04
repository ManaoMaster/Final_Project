using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; // ใช้ Entity Tables
using System;
using System.Threading;
using System.Threading.Tasks;
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using ProjectHub.Application.Repositories; // <-- [ADD] เพิ่ม Using นี้

namespace ProjectHub.Application.Features.Tables.CreateTable
{
    // Handler รับ CreateTableCommand และคืนค่า TableResponseDto
    public class CreateTableHandler : IRequestHandler<CreateTableCommand, TableResponseDto>
    {
        // Inject Dependencies ที่ต้องการ
        private readonly ITableRepository _tableRepository;
        private readonly IProjectRepository _projectRepository; // เพิ่มเข้ามาเพื่อเช็ค Project
        private readonly IMapper _mapper;

        private readonly IColumnRepository _columnRepository;

        private readonly IProjectSecurityService _securityService;
        // Constructor Injection
        public CreateTableHandler(
            ITableRepository tableRepository,
            IProjectRepository projectRepository, // เพิ่ม Project Repository
            IMapper mapper,
            IProjectSecurityService securityService,
            IColumnRepository columnRepository
        )
        {
            _tableRepository = tableRepository;
            _projectRepository = projectRepository; // เก็บค่า
            _mapper = mapper;
            _securityService = securityService;
            _columnRepository = columnRepository;
        }

        public async Task<TableResponseDto> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            await _securityService.ValidateProjectAccessAsync(request.ProjectId);
            
            // --- Business Logic ---
            // 1. ตรวจสอบว่า ProjectId มีอยู่จริง
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

            // 3. ตรวจสอบชื่อตารางซ้ำ
            var isDuplicate = await _tableRepository.IsTableNameUniqueForProjectAsync(request.ProjectId, request.Name);
            if (isDuplicate)
            {
                throw new ArgumentException($"Table name '{request.Name}' is already in use in this project.");
            }

            // --- Mapping & Persistence ---
            // 4. ใช้ AutoMapper แปลง Command -> Entity Tables
            var tableEntity = _mapper.Map<ProjectHub.Domain.Entities.Tables>(request);
           
            // 5. เรียก Repository เพื่อบันทึก Entity ลง Database
            await _tableRepository.AddTableAsync(tableEntity);
            // (ตอนนี้ tableEntity.Id หรือ Table_id ควรมิค่าแล้ว)
            Console.WriteLine($"Created Table with ID: {tableEntity.Table_id}");
            if (request.UseAutoIncrement)
            {
                Console.WriteLine("Adding default PK column 'ID' with AUTO_INCREMENT.");
                // เราจะ "แถมฟรี" คอลัมน์ PK ให้ 1 อัน
                var pkColumn = new ColumnEntity
                {
                    Table_id = tableEntity.Table_id, // ใช้ ID ของตารางที่เราเพิ่งสร้าง
                    Name = "ID", // ตั้งชื่อ Default ว่า "ID"
                    Data_type = "INTEGER",
                    Is_primary = true,
                    Is_nullable = false,
                    PrimaryKeyType = "AUTO_INCREMENT"
                };
                
                await _columnRepository.AddColumnAsync(pkColumn);
            }

            // vvv --- [FIX 2] ย้าย 2 บรรทัดนี้ออกมา "นอก" if --- vvv
            //
            // 6. ใช้ AutoMapper แปลง Entity ที่บันทึกแล้ว -> Response DTO
            var responseDto = _mapper.Map<TableResponseDto>(tableEntity);

            return responseDto; // <-- ต้อง return ทุกครั้ง ไม่ว่า 'if' จะทำงานหรือไม่
        }
    }
}