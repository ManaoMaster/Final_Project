using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using ProjectHub.Domain.Entities; 
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Interfaces; // <-- [!] เอา IApplicationDbContext ออกจากที่นี่ (ถ้ามันเคยอยู่)
using ProjectHub.Application.Dtos;
using System.Linq;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, ColumnResponseDto>
    {
        // --- [FIX 1] ---
        // เอา _context ออกจากตัวแปร
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
            // --- [FIX 2] ---
            // เอา IApplicationDbContext ออกจาก Constructor
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
            // --- [FIX 3] ---
            // ลบ try...catch และ Transaction ออกจาก Handler
            // (เพราะ Repository จะจัดการ Transaction เอง)

            await _securityService.ValidateTableAccessAsync(request.TableId);
            
            // ... (โค้ด Validation อื่นๆ ของคุณ เช่น ชื่อซ้ำ, PK ซ้ำ) ...
            
            // Map Entity (ทำแค่ครั้งเดียว)
            var columnEntity = _mapper.Map<ProjectHub.Domain.Entities.Columns>(request);
            
            // --- [FIX 4] *** แก้ไข Logic การตัดสินใจ *** ---
            if (request.DataType.Equals("Lookup", StringComparison.OrdinalIgnoreCase))
            {
                // ... (Validation ของ TargetColumnId เหมือนเดิม) ...

                if (request.LookupRelationshipId != null)
                {
                    // --- "ไข่" (ID มีอยู่แล้ว) ---
                    var relationship = await _relationshipRepository.GetByIdAsync(request.LookupRelationshipId.Value);
                    if (relationship == null)
                        throw new ArgumentException($"Relationship with ID {request.LookupRelationshipId} not found.");
                    
                    // เรียกใช้ Method ธรรมดา
                    await _columnRepository.AddColumnAsync(columnEntity); 
                }
                else if (request.NewRelationship != null)
                {
                    // --- "ไก่" (สร้าง Relation ใหม่) ---
                    var relData = request.NewRelationship;
                    var newRelationship = new Domain.Entities.Relationships
                    {
                        PrimaryTableId = relData.PrimaryTableId,
                        PrimaryColumnId = relData.PrimaryColumnId,
                        ForeignTableId = relData.ForeignTableId,
                        ForeignColumnId = relData.ForeignColumnId,
                        DisplayName = request.Name // (ใช้ Column Name เป็น DisplayName)
                    };

                    // *** เรียกใช้ Method ใหม่ที่จัดการ Transaction ใน Repository ***
                    columnEntity = await _columnRepository.CreateColumnWithNewRelationshipAsync(columnEntity, newRelationship);
                }
                else
                {
                    throw new ArgumentException("For Lookup columns, you must provide either 'LookupRelationshipId' or 'NewRelationship'.");
                }
            }
            else if (request.DataType.Equals("Formula", StringComparison.OrdinalIgnoreCase))
            {
                // --- "Formula" (ธรรมดา) ---
                // ... (Validation Formula ของคุณ) ...
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