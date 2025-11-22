using MediatR;
using ProjectHub.Application.Repositories; 
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId
{
    public class GetPrimaryColumnsByTableIdQueryHandler : IRequestHandler<GetPrimaryColumnsByTableIdQuery, IEnumerable<ColumnEntity>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService;

        public GetPrimaryColumnsByTableIdQueryHandler(IColumnRepository columnRepository, IProjectSecurityService securityService)
        {
            _columnRepository = columnRepository;
            _securityService = securityService;
        }

        public async Task<IEnumerable<ColumnEntity>> Handle(GetPrimaryColumnsByTableIdQuery request, CancellationToken cancellationToken)
        {
            await _securityService.ValidateTableAccessAsync(request.TableId);
            
            var allColumns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);

            
            
            var primaryColumns = allColumns.Where(c => c.Is_primary);

            
            return primaryColumns;
        }
    }
}