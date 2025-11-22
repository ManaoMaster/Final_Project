using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories; 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Features.Tables.GetAllTablesByProjectId;

namespace ProjectHub.Application.Features.Tables.GetAllTableByProjectId
{
    public class GetAllTablesByProjectIdHandler : IRequestHandler<GetAllTablesByProjectIdQuery, IEnumerable<TableResponseDto>>
    {
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;
        private readonly IProjectSecurityService _securityService;

        public GetAllTablesByProjectIdHandler(ITableRepository tableRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<IEnumerable<TableResponseDto>> Handle(GetAllTablesByProjectIdQuery request, CancellationToken cancellationToken)
        {
            
            await _securityService.ValidateProjectAccessAsync(request.ProjectId); 

            
            var tables = await _tableRepository.GetTablesByProjectIdAsync(request.ProjectId);

            
            return _mapper.Map<IEnumerable<TableResponseDto>>(tables);
        }
    }
}