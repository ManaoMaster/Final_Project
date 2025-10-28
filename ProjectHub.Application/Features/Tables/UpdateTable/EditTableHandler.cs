using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.UpdateTable;
using ProjectHub.Application.Interfaces; // ใช้ Alias ป้องกันชน
using ProjectHub.Application.Repositories;
using TableEntity = ProjectHub.Domain.Entities.Tables;

namespace ProjectHub.Application.Features.Tables.EditTable
{
    public class EditTableHandler : IRequestHandler<UpdateTableCommand, TableResponseDto>
    {
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;

        public EditTableHandler(ITableRepository tableRepository, IMapper mapper)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
        }

        public async Task<TableResponseDto> Handle(
            UpdateTableCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ดึงข้อมูล Table ที่ต้องการแก้ไข
            var tableToUpdate = await _tableRepository.GetTableByIdAsync(request.TableId);

            // 2. ตรวจสอบว่า Table มีอยู่จริงหรือไม่
            if (tableToUpdate == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
                // หรือใช้ NotFoundException
            }

            // (Optional แต่สำคัญ) 3. ตรวจสอบว่าชื่อใหม่ซ้ำกับ Table อื่นใน Project เดียวกันหรือไม่
            // เราต้องไม่ให้ชื่อ Table ซ้ำกันใน Project เดียวกัน
            if (
                !string.IsNullOrWhiteSpace(request.NewName)
                && !request.NewName.Equals(tableToUpdate.Name, StringComparison.OrdinalIgnoreCase)
            ) // เช็คเฉพาะถ้าชื่อใหม่ไม่เหมือนเดิม
            {
                var isDuplicate = await _tableRepository.IsTableNameUniqueForProjectAsync(
                    tableToUpdate.Project_id,
                    request.NewName
                );
                if (isDuplicate)
                {
                    throw new ArgumentException(
                        $"Table name '{request.NewName}' is already in use in this project."
                    );
                }
                // ถ้าไม่ซ้ำ ก็อัปเดตชื่อ
                tableToUpdate.Name = request.NewName;
            }
            // (ถ้า NewName ว่าง หรือเหมือนเดิม ก็ไม่ต้องทำอะไรกับชื่อ)

            // (Optional) ตรวจสอบ Business Rule อื่นๆ เช่น แก้ไข Property อื่นๆ ถ้า Command มี

            // 4. เรียก Repository เพื่อบันทึกการเปลี่ยนแปลง
            // (ถ้า GetTableByIdAsync ดึง Entity ที่ Tracked มา EF Core จะรู้ว่ามีการเปลี่ยนแปลง)
            await _tableRepository.EditTableAsync(tableToUpdate); // เราต้องเพิ่มเมธอด UpdateTableAsync ใน Interface/Repo

            // 5. Map Entity ที่อัปเดตแล้ว กลับเป็น DTO เพื่อส่งคืน
            return _mapper.Map<TableResponseDto>(tableToUpdate);
        }
    }
}
