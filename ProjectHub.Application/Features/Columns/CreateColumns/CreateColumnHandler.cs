

































































































































































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

            
            var columnEntity = _mapper.Map<ProjectHub.Domain.Entities.Columns>(request);

            
            columnEntity.LookupTargetColumnId = request.LookupTargetColumnId;

            

            
            if (request.DataType.Equals("LOOKUP", StringComparison.OrdinalIgnoreCase))
            {
                
                if (request.LookupTargetColumnId == null)
                    throw new ArgumentException("LookupTargetColumnId is required.");

                var targetDisplayColumn = await _columnRepository.GetColumnByIdAsync(request.LookupTargetColumnId.Value);
                if (targetDisplayColumn == null)
                    throw new ArgumentException(
                        $"Lookup Target Column with ID {request.LookupTargetColumnId} not found.");

                
                if (request.LookupRelationshipId != null)
                {
                    
                    var relationship = await _relationshipRepository.GetByIdAsync(request.LookupRelationshipId.Value);
                    if (relationship == null)
                        throw new ArgumentException(
                            $"Relationship with ID {request.LookupRelationshipId} not found.");

                    await _columnRepository.AddColumnAsync(columnEntity);
                }
                else if (request.NewRelationship != null)
                {
                    
                    var relData = request.NewRelationship;

                    
                    var targetColumns = (await _columnRepository.GetColumnsByTableIdAsync(relData.PrimaryTableId)).ToList();
                    var targetPk = targetColumns.FirstOrDefault(c => c.Is_primary);
                    if (targetPk == null)
                    {
                        throw new Exception(
                            $"Target table {relData.PrimaryTableId} has no primary key column.");
                    }

                    
                    var newRelationship =  new Domain.Entities.Relationships
                    {
                        PrimaryTableId = relData.PrimaryTableId,     
                        PrimaryColumnId = targetPk.Column_id,         
                        ForeignTableId = request.TableId,            
                        
                        DisplayName = request.Name,               
                        Notes = null
                    };

                    
                    
                    
                    
                    columnEntity = await _columnRepository.CreateColumnWithNewRelationshipAsync(
                        columnEntity, newRelationship);
                }
                else
                {
                    
                    throw new ArgumentException(
                        "For Lookup columns, you must provide either 'LookupRelationshipId' or 'NewRelationship'.");
                }
            }
            
            else if (request.DataType.Equals("FORMULA", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.FormulaDefinition))
                {
                    throw new Exception("FormulaDefinition is required for Formula columns.");
                }

                
                var columnNames = _formulaTranslator.GetReferencedColumnNames(request.FormulaDefinition);
                if (!columnNames.Any())
                {
                    throw new Exception("Formula does not reference any columns.");
                }

                
                foreach (var name in columnNames)
                {
                    var colInfo = await _columnRepository.GetColumnByNameAsync(request.TableId, name);
                    if (colInfo == null)
                    {
                        throw new Exception($"Formula Error: Column '{name}' not found in this table.");
                    }

                    var dataType = (colInfo.Data_type ?? string.Empty).ToLower();
                    if (dataType != "integer" && dataType != "long" &&
                        dataType != "real" && dataType != "number" &&
                        dataType != "lookup")
                    {
                        throw new Exception(
                            $"Formula Error: Column '{name}' is not a numeric type (or Lookup).");
                    }
                }

                await _columnRepository.AddColumnAsync(columnEntity);
            }
            
            else
            {
                await _columnRepository.AddColumnAsync(columnEntity);
            }

            
            var responseDto = _mapper.Map<ColumnResponseDto>(columnEntity);
            return responseDto;
        }
    }
}
