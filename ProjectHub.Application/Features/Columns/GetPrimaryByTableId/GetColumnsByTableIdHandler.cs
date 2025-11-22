








































using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Columns.GetPrimaryByTableId
{
    public class GetColumnsByTableIdQueryHandler
     : IRequestHandler<GetColumnsByTableIdQuery, IEnumerable<ColumnResponseDto>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IProjectSecurityService _securityService;
        private readonly IMapper _mapper;

        public GetColumnsByTableIdQueryHandler(
            IColumnRepository columnRepository,
            IProjectSecurityService securityService,
            IMapper mapper)
        {
            _columnRepository = columnRepository;
            _securityService = securityService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ColumnResponseDto>> Handle(
            GetColumnsByTableIdQuery request,
            CancellationToken cancellationToken)
        {
            await _securityService.ValidateTableAccessAsync(request.TableId);

            var allColumns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);

            return _mapper.Map<IEnumerable<ColumnResponseDto>>(allColumns);
        }
    }
}
