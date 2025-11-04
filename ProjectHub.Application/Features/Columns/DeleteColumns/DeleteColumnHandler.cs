using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces; // 1. [ADD] Import "Yara"
using ProjectHub.Application.Repositories;
using ProjectHub.Application.Features.Columns.DeleteColumn; // (Assuming this is correct)

namespace ProjectHub.Application.Features.Columns.DeleteColumn // (Ensured namespace matches)
{
    // Handler for DeleteColumnCommand, returns Unit (nothing)
    public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService; // 2. [ADD] Inject "Yara"

        public DeleteColumnHandler(
            IColumnRepository columnRepository, 
            IProjectSecurityService securityService) // 3. [ADD] Receive "Yara"
        {
            _columnRepository = columnRepository;
            _securityService = securityService; // 4. [ADD] Assign "Yara"
        }

        public async Task<Unit> Handle(
            DeleteColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            // 5. [OPTIMIZE] Fetch the column once
            var columnToDelete = await _columnRepository.GetColumnByIdAsync(request.ColumnId);
            
            if (columnToDelete == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
            }

            // 6. [ADD] Call "Yara" to validate using the Table_id we just found
            await _securityService.ValidateTableAccessAsync(columnToDelete.Table_id); // (or .TableId)

            // 7. (If OK) Call the repository to delete the column by ID
            await _columnRepository.DeleteColumnAsync(request.ColumnId);

            // 8. Return Unit.Value to indicate success
            return Unit.Value;
        }
    }
}