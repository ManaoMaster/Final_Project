using AutoMapper;
using ProjectHub.API.Contracts.Users;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.Application.Features.Users.Register;
using ProjectHub.Application.Features.Projects.CreateProject;

namespace ProjectHub.API.Mapping
{
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            // API -> Application (Users)
            CreateMap<RegisterUserRequest, RegisterUserCommand>();

            // API -> Application (Projects)
            CreateMap<CreateProjectRequest, CreateProjectCommand>();
        }
    }
}
