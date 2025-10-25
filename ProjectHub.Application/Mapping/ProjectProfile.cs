using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Projects.CreateProject;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            // Domain -> DTO
            CreateMap<Projects, ProjectResponseDto>()
                .ForMember(d => d.ProjectId, m => m.MapFrom(s => s.Project_id))
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.User_id))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at));

            // Command -> Domain (เพื่อใช้ใน Handler)
            CreateMap<CreateProjectCommand, Projects>()
                .ForMember(d => d.Project_id, m => m.Ignore())
                .ForMember(d => d.Created_at, m => m.Ignore())
                .ForMember(d => d.User_id, m => m.MapFrom(s => s.UserId))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name));
        }
    }
}
