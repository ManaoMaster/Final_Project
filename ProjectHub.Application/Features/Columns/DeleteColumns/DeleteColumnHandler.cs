using System; 
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories;
using ProjectHub.Application.Features.Columns.DeleteColumn; 

namespace ProjectHub.Application.Features.Columns.DeleteColumn 
{
    
    public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService; 

        public DeleteColumnHandler(
            IColumnRepository columnRepository, 
            IProjectSecurityService securityService) 
        {
            _columnRepository = columnRepository;
            _securityService = securityService; 
        }

        public async Task<Unit> Handle(
            DeleteColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var columnToDelete = await _columnRepository.GetColumnByIdAsync(request.ColumnId);
            
            if (columnToDelete == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
            }

            
            await _securityService.ValidateTableAccessAsync(columnToDelete.Table_id); 

            
            await _columnRepository.DeleteColumnAsync(request.ColumnId);

            
            return Unit.Value;
        }
    }
}