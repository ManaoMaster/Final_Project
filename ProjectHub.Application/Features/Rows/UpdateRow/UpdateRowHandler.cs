using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Text.Json; 
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Application.Validation;


using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using RowEntity = ProjectHub.Domain.Entities.Rows;
using UpdateRowCmd = ProjectHub.Application.Features.Rows.UpdateRow.UpdateRowCommand; 

namespace ProjectHub.Application.Features.Rows.UpdateRow
{
    
    public class UpdateRowHandler : IRequestHandler<UpdateRowCmd, RowResponseDto>
    {
        private readonly IRowRepository _rowRepository;
        private readonly IColumnRepository _columnRepository; 
        private readonly IMapper _mapper;


        private readonly IProjectSecurityService _securityService;

        public UpdateRowHandler(
            IRowRepository rowRepository,
            IColumnRepository columnRepository,
            IMapper mapper,
            IProjectSecurityService securityService
        )
        {
            _rowRepository = rowRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<RowResponseDto> Handle(
            UpdateRowCmd request,
            CancellationToken cancellationToken
        )
        {
            
            var rowToUpdate = await _rowRepository.GetRowByIdAsync(request.RowId);
            if (rowToUpdate == null)
            {
                throw new ArgumentException($"Row with ID {request.RowId} not found.");
            }

            await _securityService.ValidateTableAccessAsync(rowToUpdate.Table_id);

            
            var columnsSchema = await _columnRepository.GetColumnsByTableIdAsync(
                rowToUpdate.Table_id
            );
            if (columnsSchema == null || !columnsSchema.Any())
            {
                
                throw new InvalidOperationException(
                    $"Could not find column schema for Table ID {rowToUpdate.Table_id}."
                );
            }

            
            try
            {
                JsonDataValidator.Validate(request.NewData, columnsSchema.ToList());
            }
            catch (ArgumentException ex)
            {
                
                throw new ArgumentException($"Invalid data provided: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format provided: {ex.Message}", ex);
            }

            
            
            _mapper.Map(request, rowToUpdate);

            
            await _rowRepository.UpdateRowAsync(rowToUpdate);

            
            return _mapper.Map<RowResponseDto>(rowToUpdate);
        }
    }
}
 