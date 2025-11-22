using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Columns.UpdateColumn
{
    public class UpdateColumnHandler : IRequestHandler<UpdateColumnCommand, ColumnResponseDto>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IMapper _mapper;

        private readonly IProjectSecurityService _securityService;

        public UpdateColumnHandler(IColumnRepository columnRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _columnRepository = columnRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<ColumnResponseDto> Handle(
            UpdateColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var columnToUpdate = await _columnRepository.GetColumnByIdAsync(request.ColumnId);

            
            if (columnToUpdate == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
                
            }

            await _securityService.ValidateTableAccessAsync(columnToUpdate.Table_id);

            
            
            
            

            
            _mapper.Map(request, columnToUpdate);

            
            await _columnRepository.UpdateColumnAsync(columnToUpdate);

            
            var responseDto = _mapper.Map<ColumnResponseDto>(columnToUpdate);

            return responseDto;
        }
    }
}
