using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Rows.DeleteRow; 
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Rows.DeleteRow 
{
    public class DeleteRowHandler : IRequestHandler<DeleteRowCommand, Unit>
    {
        private readonly IRowRepository _rowRepository;
        private readonly IProjectSecurityService _securityService; 

        public DeleteRowHandler(
            IRowRepository rowRepository, 
            IProjectSecurityService securityService) 
        {
            _rowRepository = rowRepository;
            _securityService = securityService; 
        }

        public async Task<Unit> Handle(
            DeleteRowCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var rowToDelete = await _rowRepository.GetRowByIdAsync(request.RowId); 
            
            if (rowToDelete == null)
            {
                throw new ArgumentException($"Row with ID {request.RowId} not found.");
            }

            
            
            
            
            await _securityService.ValidateTableAccessAsync(rowToDelete.Table_id); 

            
            await _rowRepository.DeleteRowAsync(request.RowId);

            return Unit.Value;
        }
    }
}