using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHubHub.Application.Features.Rows.DeleteRow
{
    public class DeleteRowHandler : IRequestHandler<DeleteRowCommand, Unit>
    {
        private readonly IRowRepository _rowRepository;

        public DeleteRowHandler(IRowRepository rowRepository)
        {
            _rowRepository = rowRepository;
        }

        public async Task<Unit> Handle(
            DeleteRowCommand request,
            CancellationToken cancellationToken
        )
        {
            // Optional but recommended: Check if the row exists before deleting
            // This provides a clearer error message than letting the repository handle it silently
            var rowExists = await _rowRepository.GetRowByIdAsync(request.RowId);
            if (rowExists == null)
            {
                throw new ArgumentException($"Row with ID {request.RowId} not found.");
                // Or use a custom NotFoundException
            }

            // Call the repository to delete the row by ID
            await _rowRepository.DeleteRowAsync(request.RowId);

            // Return Unit.Value to indicate success with no return data
            return Unit.Value;
        }
    }
}
