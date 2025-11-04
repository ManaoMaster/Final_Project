using System;
using System.Collections.Generic; // For List<>
using System.Linq; // For Any()
using System.Text.Json; // For JSON parsing
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Application.Validation;

// Using Alias to prevent namespace collision
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using RowEntity = ProjectHub.Domain.Entities.Rows;
using UpdateRowCmd = ProjectHub.Application.Features.Rows.UpdateRow.UpdateRowCommand; // Alias for Command

namespace ProjectHub.Application.Features.Rows.UpdateRow
{
    // Handler for UpdateRowCommand, returns the updated RowResponseDto
    public class UpdateRowHandler : IRequestHandler<UpdateRowCmd, RowResponseDto>
    {
        private readonly IRowRepository _rowRepository;
        private readonly IColumnRepository _columnRepository; // Needed for schema validation
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
            // 1. Fetch the existing Row
            var rowToUpdate = await _rowRepository.GetRowByIdAsync(request.RowId);
            if (rowToUpdate == null)
            {
                throw new ArgumentException($"Row with ID {request.RowId} not found.");
            }

            await _securityService.ValidateTableAccessAsync(rowToUpdate.Table_id);

            // 2. Fetch the Column Schema for the associated Table
            var columnsSchema = await _columnRepository.GetColumnsByTableIdAsync(
                rowToUpdate.Table_id
            );
            if (columnsSchema == null || !columnsSchema.Any())
            {
                // This shouldn't happen if the table exists, but good to check
                throw new InvalidOperationException(
                    $"Could not find column schema for Table ID {rowToUpdate.Table_id}."
                );
            }

            // 3. Validate the New JSON Data against the Schema
            try
            {
                JsonDataValidator.Validate(request.NewData, columnsSchema.ToList());
            }
            catch (ArgumentException ex)
            {
                // Re-throw validation errors to be caught by the controller
                throw new ArgumentException($"Invalid data provided: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format provided: {ex.Message}", ex);
            }

            // 4. Update the Row Entity using AutoMapper
            // AutoMapper will map request.NewData -> rowToUpdate.Data based on RowProfile
            _mapper.Map(request, rowToUpdate);

            // 5. Call the Repository to Save Changes
            await _rowRepository.UpdateRowAsync(rowToUpdate);

            // 6. Map the updated Entity back to DTO and return
            return _mapper.Map<RowResponseDto>(rowToUpdate);
        }
    }
}
 