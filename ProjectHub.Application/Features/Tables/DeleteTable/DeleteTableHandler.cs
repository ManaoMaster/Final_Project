using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace TableHub.Application.Features.Tables.DeleteTable
{
    // Handler for DeleteTableCommand, returns Unit (nothing)
    public class DeleteTableHandler : IRequestHandler<DeleteTableCommand, Unit>
    {
        private readonly ITableRepository _tableRepository;

        public DeleteTableHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<Unit> Handle(
            DeleteTableCommand request,
            CancellationToken cancellationToken
        )
        {
            // Optional but recommended: Check if the table exists before deleting
            // This provides a clearer error message than letting the repository handle it silently
            var tableExists = await _tableRepository.GetTableByIdAsync(request.TableId);
            if (tableExists == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
                // Or use a custom NotFoundException
            }

            // Call the repository to delete the table by ID
            await _tableRepository.DeleteTableAsync(request.TableId);

            // Return Unit.Value to indicate success with no return data
            return Unit.Value;
        }
    }
}
