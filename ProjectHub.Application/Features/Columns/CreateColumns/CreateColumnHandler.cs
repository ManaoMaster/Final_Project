using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using ProjectHub.Domain.Entities; 
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Dtos;
using System.Linq;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, ColumnResponseDto>
    {
        // (Constructor และ Property ทั้งหมดของคุณถูกต้องอยู่แล้ว)
        private readonly IFormulaTranslator _formulaTranslator;
        private readonly IColumnRepository _columnRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IProjectSecurityService _securityService;

        public CreateColumnHandler(
            IColumnRepository columnRepository,
            ITableRepository tableRepository,
            IMapper mapper,
            IFormulaTranslator formulaTranslator,
            IRelationshipRepository relationshipRepository,
            IProjectSecurityService securityService)
        {
            _formulaTranslator = formulaTranslator;
            _columnRepository = columnRepository;
            _tableRepository = tableRepository;
            _mapper = mapper;
            _relationshipRepository = relationshipRepository;
            _securityService = securityService;
        }


        public async Task<ColumnResponseDto> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
        {
            await _securityService.ValidateTableAccessAsync(request.TableId);
            
            // 1. Validation Input พื้นฐาน (ที่คุณมีอยู่แล้ว)
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required.");
            if (string.IsNullOrWhiteSpace(request.DataType))
                throw new ArgumentException("Data type is required.");
            
            var isDuplicateName = await _columnRepository.IsColumnNameUniqueForTableAsync(request.TableId, request.Name);
            if (isDuplicateName)
                throw new ArgumentException($"Column name '{request.Name}' already exists.");

            if (request.IsPrimary)
            {
                var hasPrimaryKey = await _columnRepository.HasPrimaryKeyAsync(request.TableId);
                if (hasPrimaryKey)
                    throw new ArgumentException("This table already has a primary key.");
            }
            
            // 2. Map Entity (ทำแค่ครั้งเดียว)
            var columnEntity = _mapper.Map<ProjectHub.Domain.Entities.Columns>(request);
            
            // 3. --- [FIX] *** Logic การตัดสินใจ ที่รวมทุกอย่างแล้ว *** ---
            if (request.DataType.Equals("Lookup", StringComparison.OrdinalIgnoreCase))
            {
                // --- Logic "LOOKUP" (ไก่กับไข่) ---
                
                // (Validation TargetColumnId)
                if (request.LookupTargetColumnId == null)
                    throw new ArgumentException("LookupTargetColumnId is required.");
                
                var targetColumn = await _columnRepository.GetColumnByIdAsync(request.LookupTargetColumnId.Value);
                if (targetColumn == null)
                    throw new ArgumentException($"Lookup Target Column with ID {request.LookupTargetColumnId} not found.");

                // (ตัดสินใจ ไก่ หรือ ไข่)
                if (request.LookupRelationshipId != null)
                {
                    // "ไข่" (ID มีอยู่แล้ว)
                    var relationship = await _relationshipRepository.GetByIdAsync(request.LookupRelationshipId.Value);
                    if (relationship == null)
                        throw new ArgumentException($"Relationship with ID {request.LookupRelationshipId} not found.");
                    
                    await _columnRepository.AddColumnAsync(columnEntity); 
                }
                else if (request.NewRelationship != null)
                {
                    // "ไก่" (สร้าง Relation ใหม่)
                    var relData = request.NewRelationship;
                    var newRelationship = new Domain.Entities.Relationships
                    {
                        PrimaryTableId = relData.PrimaryTableId,
                        PrimaryColumnId = relData.PrimaryColumnId,
                        ForeignTableId = relData.ForeignTableId,
                        ForeignColumnId = relData.ForeignColumnId,
                        DisplayName = request.Name 
                    };

                    columnEntity = await _columnRepository.CreateColumnWithNewRelationshipAsync(columnEntity, newRelationship);
                }
                else
                {
                    throw new ArgumentException("For Lookup columns, you must provide either 'LookupRelationshipId' or 'NewRelationship'.");
                }
            }
            else if (request.DataType.Equals("Formula", StringComparison.OrdinalIgnoreCase))
            {
                // --- Logic "FORMULA" (ที่ผมเผลอลบไป) ---
                
                // *** [RE-ADD] นี่คือส่วนที่ใช้ _formulaTranslator ***
                if (string.IsNullOrWhiteSpace(request.FormulaDefinition))
                {
                    throw new Exception("FormulaDefinition is required for Formula columns.");
                }

                // 1. ดึงชื่อคอลัมน์ทั้งหมดที่สูตรนี้อ้างอิง
                var columnNames = _formulaTranslator.GetReferencedColumnNames(request.FormulaDefinition);

                if (!columnNames.Any())
                {
                    throw new Exception("Formula does not reference any columns.");
                }

                // 2. ตรวจสอบทีละคอลัมน์
                foreach (var name in columnNames)
                {
                    var colInfo = await _columnRepository.GetColumnByNameAsync(request.TableId, name);

                    if (colInfo == null)
                    {
                        throw new Exception($"Formula Error: Column '{name}' not found in this table.");
                    }

                    // ตรวจสอบว่าคอลัมน์ที่อ้างอิงเป็นตัวเลข หรือ Lookup (ซึ่งจะคืนค่าตัวเลข)
                    var dataType = colInfo.Data_type.ToLower(); 
                    if (dataType != "integer" && dataType != "long" && 
                        dataType != "real" && dataType != "number" && 
                        dataType != "lookup") 
                    {
                        throw new Exception($"Formula Error: Column '{name}' is not a numeric type (or Lookup).");
                    }
                }
                // *** [End of RE-ADD] ***

                // บันทึก Column Formula ธรรมดา
                await _columnRepository.AddColumnAsync(columnEntity);
            }
            else
            {
                // --- "Column ธรรมดา" ---
                await _columnRepository.AddColumnAsync(columnEntity);
            }

            // 4. ส่ง Response กลับ
            var responseDto = _mapper.Map<ColumnResponseDto>(columnEntity);
            return responseDto;
        }
    }
}