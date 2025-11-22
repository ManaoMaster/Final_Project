using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.UpdateTable;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories;
using TableEntity = ProjectHub.Domain.Entities.Tables;

namespace ProjectHub.Application.Features.Tables.EditTable
{
    public class EditTableHandler : IRequestHandler<UpdateTableCommand, TableResponseDto>
    {
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;

        private readonly IProjectSecurityService _securityService;

        public EditTableHandler(ITableRepository tableRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<TableResponseDto> Handle(
            UpdateTableCommand request,
            CancellationToken cancellationToken

        )
        {

            
            var tableToUpdate = await _tableRepository.GetTableByIdAsync(request.TableId);

            
            if (tableToUpdate == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
                
            }
            await _securityService.ValidateProjectAccessAsync(tableToUpdate.Project_id); 

            
            
            if (
                !string.IsNullOrWhiteSpace(request.NewName)
                && !request.NewName.Equals(tableToUpdate.Name, StringComparison.OrdinalIgnoreCase)
            ) 
            {
                var isDuplicate = await _tableRepository.IsTableNameUniqueForProjectAsync(
                    tableToUpdate.Project_id,
                    request.NewName
                );
                if (isDuplicate)
                {
                    throw new ArgumentException(
                        $"Table name '{request.NewName}' is already in use in this project."
                    );
                }
                
                tableToUpdate.Name = request.NewName;
            }
            

            

            
            
            await _tableRepository.UpdateTableAsync(tableToUpdate); 

            
            return _mapper.Map<TableResponseDto>(tableToUpdate);
        }
    }
}
