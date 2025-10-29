using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Columns.DeleteColumn
{
    // Handler for DeleteColumnCommand, returns Unit (nothing)
    public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
    {
        private readonly IColumnRepository _columnRepository;

        public DeleteColumnHandler(IColumnRepository columnRepository)
        {
            _columnRepository = columnRepository;
        }

        public async Task<Unit> Handle(
            DeleteColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            // Optional but recommended: Check if the column exists before deleting
            // This provides a clearer error message than letting the repository handle it silently
            var columnExists = await _columnRepository.GetColumnByIdAsync(request.ColumnId);
            if (columnExists == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
                // Or use a custom NotFoundException
            }

            // Call the repository to delete the column by ID
            await _columnRepository.DeleteColumnAsync(request.ColumnId);

            // Return Unit.Value to indicate success with no return data
            return Unit.Value;
        }
    }
}
