using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories; 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;





namespace ProjectHub.Application.Features.Projects.GetAllProjects
{
    public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, IEnumerable<ProjectResponseDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        

        public GetAllProjectsHandler(IProjectRepository projectRepository, IMapper mapper) 
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectResponseDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            
            
            var userId = request.UserId; 

            
            var projects = await _projectRepository.GetProjectsByUserIdAsync(userId);

            
            return _mapper.Map<IEnumerable<ProjectResponseDto>>(projects);
        }
        
        
    }
}