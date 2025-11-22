using System.Linq;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            
            CreateMap<Projects, ProjectResponseDto>()
                .ForMember(d => d.ProjectId, m => m.MapFrom(s => s.Project_id))
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.User_id))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at))
                .ForMember(d => d.UpdatedAt, m => m.MapFrom(s => s.UpdatedAt))
                .ForMember(d => d.IsFavorite, m => m.MapFrom(s => s.IsFavorite))
                .ForMember(d => d.TableCount, m => m.MapFrom(s => s.Tables.Count));

            
            CreateMap<CreateProjectCommand, Projects>()
                .ForMember(d => d.Project_id, m => m.Ignore()) 
                .ForMember(d => d.Created_at, m => m.Ignore()) 
                .ForMember(d => d.User_id, m => m.MapFrom(s => s.UserId))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                
                .ForMember(d => d.Tables, m => m.Ignore())
                .ForMember(d => d.Users, m => m.Ignore()); 

            
            
            
            
            
            
            

            
            CreateMap<UpdateProjectCommand, Projects>()
                .ForMember(d => d.Name, m => m.MapFrom(s => s.NewName))
                .ForMember(d => d.Project_id, m => m.Ignore())
                .ForMember(d => d.Created_at, m => m.Ignore())
                .ForMember(d => d.UpdatedAt, m => m.Ignore()) 
                .ForMember(d => d.Tables, m => m.Ignore())
                .ForMember(d => d.Users, m => m.Ignore());
        }
    }
}
