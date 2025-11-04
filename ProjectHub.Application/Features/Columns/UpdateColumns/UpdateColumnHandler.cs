using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Columns.UpdateColumn
{
    public class UpdateColumnHandler : IRequestHandler<UpdateColumnCommand, ColumnResponseDto>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IMapper _mapper;

        private readonly IProjectSecurityService _securityService;

        public UpdateColumnHandler(IColumnRepository columnRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _columnRepository = columnRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<ColumnResponseDto> Handle(
            UpdateColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ดึงข้อมูล Project เดิมจาก ID
            var columnToUpdate = await _columnRepository.GetColumnByIdAsync(request.ColumnId);

            // 2. ตรวจสอบว่าเจอหรือไม่
            if (columnToUpdate == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
                // หรืออาจจะใช้ Exception ที่ Custom ขึ้นมาเอง เช่น NotFoundException
            }

            await _securityService.ValidateTableAccessAsync(columnToUpdate.Table_id);

            // (Optional) ตรวจสอบ Business Rule เพิ่มเติม
            // เช่น เช็คว่าชื่อใหม่ซ้ำหรือไม่ (ถ้าต้องการ) โดยเรียก IsProjectNameUniqueForUserAsync
            // var isDuplicate = await _projectRepository.IsProjectNameUniqueForUserAsync(projectToUpdate.User_id, request.NewName);
            // if (isDuplicate) { throw new ArgumentException(...); }

            // 3. ใช้ AutoMapper เพื่ออัปเดตเฉพาะ Field ที่ Map ไว้ (คือ Name)
            _mapper.Map(request, columnToUpdate);

            // 4. เรียก Repository เพื่อบันทึกการเปลี่ยนแปลง
            await _columnRepository.UpdateColumnAsync(columnToUpdate);

            // 5. Map Entity ที่อัปเดตแล้ว กลับเป็น DTO เพื่อส่งคืน
            var responseDto = _mapper.Map<ColumnResponseDto>(columnToUpdate);

            return responseDto;
        }
    }
}
