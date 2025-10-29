using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities; // ใช้ Entity Rows และ Columns
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // สำหรับ JSON Validation
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Validation;

// ใช้ Alias ป้องกัน Namespace Collision กับ Features.Columns
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using RowEntity = ProjectHub.Domain.Entities.Rows;
using TableEntity = ProjectHub.Domain.Entities.Tables;


namespace ProjectHub.Application.Features.Rows.CreateRow
{
    public class CreateRowHandler : IRequestHandler<CreateRowCommand, RowResponseDto>
    {
        private readonly IRowRepository _rowRepository;
        private readonly ITableRepository _tableRepository; // Inject เพื่อเช็ค Table
        private readonly IColumnRepository _columnRepository; // Inject เพื่อดึง Schema
        private readonly IMapper _mapper;

        public CreateRowHandler(
            IRowRepository rowRepository,
            ITableRepository tableRepository,
            IColumnRepository columnRepository,
            IMapper mapper)
        {
            _rowRepository = rowRepository;
            _tableRepository = tableRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
        }

        public async Task<RowResponseDto> Handle(CreateRowCommand request, CancellationToken cancellationToken)
        {
            // --- 1. ตรวจสอบว่า TableId มีอยู่จริง ---
            var tableExists = await _tableRepository.GetTableByIdAsync(request.TableId);
            if (tableExists == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
            }

            // --- 2. ดึง Schema ของ Columns สำหรับ Table นี้ ---
            var columnsSchema = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);
            if (!columnsSchema.Any())
            {
                // อาจจะอนุญาตให้สร้าง Row ในตารางที่ไม่มี Column ก็ได้ แล้วแต่ Requirement
                // throw new ArgumentException($"Table with ID {request.TableId} has no columns defined.");
            }

            // --- 3. Validate JSON Data ---
            try
            {
                JsonDataValidator.Validate(request.Data, columnsSchema.ToList());
            }
            catch (JsonException jsonEx)
            {
                throw new ArgumentException($"Invalid JSON format: {jsonEx.Message}");
            }
            catch (ArgumentException argEx) // Catch Validation Errors from our method
            {
                throw argEx; // Re-throw to return 400 Bad Request
            }

            // --- 4. Mapping & Persistence ---
            // Map Command -> Entity Rows
            var rowEntity = _mapper.Map<RowEntity>(request);
            // Created_at ถูกกำหนด Default ใน Entity แล้ว

            // เรียก Repository เพื่อบันทึก
            await _rowRepository.AddRowAsync(rowEntity);

            // --- 5. Mapping & Response ---
            // Map Entity -> Response DTO
            var responseDto = _mapper.Map<RowResponseDto>(rowEntity);
            // ดึงค่า Created_at ที่ Database อาจจะ Generate ให้ (ถ้ามีการเปลี่ยนแปลง)
            responseDto.CreatedAt = rowEntity.Created_at; 

            return responseDto;
        }

        // --- Helper Method for JSON Validation ---
      
    }
}

