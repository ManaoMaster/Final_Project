using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectHub.API.Contracts.Columns;
using ProjectHub.API.Contracts.Projects;
using ProjectHub.API.Contracts.Relationships;
using ProjectHub.API.Contracts.Rows;
using ProjectHub.API.Contracts.Tables;
using ProjectHub.API.Contracts.Users;
using ProjectHub.API.Controllers;
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Features.Columns.DeleteColumn;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Application.Features.Relationships.CreateRelationship;
using ProjectHub.Application.Features.Relationships.UpdateRelationship;
using ProjectHub.Application.Features.Rows.CreateRow;
using ProjectHub.Application.Features.Rows.UpdateRow;
using ProjectHub.Application.Features.Tables.CreateTable;
using ProjectHub.Application.Features.Tables.UpdateTable;
using ProjectHub.Application.Features.Users.Register;

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

            CreateMap<CreateRowRequest, CreateRowCommand>();

            CreateMap<UpdateProjectRequest, UpdateProjectCommand>();

            CreateMap<UpdateTableRequest, UpdateTableCommand>();

            CreateMap<UpdateRowRequest, UpdateRowCommand>();

            CreateMap<CreateRelationshipRequest, CreateRelationshipCommand>();

            CreateMap<UpdateRelationshipRequest, UpdateRelationshipCommand>();
        }
    }
}
