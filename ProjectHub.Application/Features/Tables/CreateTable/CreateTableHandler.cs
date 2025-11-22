using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; 
using System;
using System.Threading;
using System.Threading.Tasks;
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using ProjectHub.Application.Repositories; 

namespace ProjectHub.Application.Features.Tables.CreateTable
{
    
    public class CreateTableHandler : IRequestHandler<CreateTableCommand, TableResponseDto>
    {
        
        private readonly ITableRepository _tableRepository;
        private readonly IProjectRepository _projectRepository; 
        private readonly IMapper _mapper;

        private readonly IColumnRepository _columnRepository;

        private readonly IProjectSecurityService _securityService;
        
        public CreateTableHandler(
            ITableRepository tableRepository,
            IProjectRepository projectRepository, 
            IMapper mapper,
            IProjectSecurityService securityService,
            IColumnRepository columnRepository
        )
        {
            _tableRepository = tableRepository;
            _projectRepository = projectRepository; 
            _mapper = mapper;
            _securityService = securityService;
            _columnRepository = columnRepository;
        }

        public async Task<TableResponseDto> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            await _securityService.ValidateProjectAccessAsync(request.ProjectId);
            
            
            
            var projectExists = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (projectExists == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
            }

            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Table name is required.");
            }

            
            var isDuplicate = await _tableRepository.IsTableNameUniqueForProjectAsync(request.ProjectId, request.Name);
            if (isDuplicate)
            {
                throw new ArgumentException($"Table name '{request.Name}' is already in use in this project.");
            }

            
            
            var tableEntity = _mapper.Map<ProjectHub.Domain.Entities.Tables>(request);
           
            
            await _tableRepository.AddTableAsync(tableEntity);
            
            Console.WriteLine($"Created Table with ID: {tableEntity.Table_id}");
            if (request.UseAutoIncrement)
            {
                Console.WriteLine("Adding default PK column 'ID' with AUTO_INCREMENT.");
                
                var pkColumn = new ColumnEntity
                {
                    Table_id = tableEntity.Table_id, 
                    Name = "ID", 
                    Data_type = "INTEGER",
                    Is_primary = true,
                    Is_nullable = false,
                    PrimaryKeyType = "AUTO_INCREMENT"
                };
                
                await _columnRepository.AddColumnAsync(pkColumn);
            }

            
            
            
            var responseDto = _mapper.Map<TableResponseDto>(tableEntity);

            return responseDto; 
        }
    }
}