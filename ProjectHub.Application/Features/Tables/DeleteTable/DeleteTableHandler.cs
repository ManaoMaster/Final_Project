using System; 
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace TableHub.Application.Features.Tables.DeleteTable
{
    
    public class DeleteTableHandler : IRequestHandler<DeleteTableCommand, Unit>
    {
        private readonly ITableRepository _tableRepository;


        private readonly IProjectSecurityService _securityService;

        public DeleteTableHandler(ITableRepository tableRepository, IProjectSecurityService securityService)
        {
            _tableRepository = tableRepository;
            _securityService = securityService;
        }

        public async Task<Unit> Handle(
            DeleteTableCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var tableToDelete = await _tableRepository.GetTableByIdAsync(request.TableId);

            if (tableToDelete == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
            }

            
            
            await _securityService.ValidateProjectAccessAsync(tableToDelete.Project_id); 

            
            await _tableRepository.DeleteTableAsync(request.TableId);

            return Unit.Value;
        }
    }
}
