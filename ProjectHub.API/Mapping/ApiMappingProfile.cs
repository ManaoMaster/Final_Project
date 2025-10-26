using AutoMapper;
using ProjectHub.API.Contracts.Users;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.API.Contracts.Tables;
using ProjectHub.Application.Features.Users.Register;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Tables.CreateTable;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectHub.API.Contracts.Columns;
using ProjectHub.Application.Features.Cplumns.CreateColumn;

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

            // API -> Appliation (Tables)
            CreateMap<CreateTableRequest, CreateTableCommand>();

            CreateMap<CreateColumnRequest, CreateColumnCommand>();
        }


    }
}
