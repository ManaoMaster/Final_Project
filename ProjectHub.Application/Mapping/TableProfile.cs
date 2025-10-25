using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Tables.CreateTable; // Command
using ProjectHub.Domain.Entities; // Entity

namespace ProjectHub.Application.Mapping
{
    public class TableProfile : Profile
    {
        public TableProfile()
        {
            // Domain -> DTO (ใช้ d, s)
            CreateMap<Tables, TableResponseDto>()
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id))
                .ForMember(d => d.ProjectId, m => m.MapFrom(s => s.Project_id))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name));

            // Command -> Domain (ใช้ d, s)
            CreateMap<CreateTableCommand, Tables>()
                .ForMember(d => d.Table_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Created_at, m => m.Ignore()) // Entity มี Default
                // *** แก้ไข: Map จาก UserId ใน Command ไปยัง Project_id ใน Entity ***
                .ForMember(d => d.Project_id, m => m.MapFrom(s => s.ProjectId)) // <-- แก้ไขบรรทัดนี้
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                // *** Ignore Navigation Properties เพื่อความชัดเจน ***
                .ForMember(d => d.Projects, m => m.Ignore()) 
                .ForMember(d => d.Columns, m => m.Ignore()) 
                .ForMember(d => d.Rows, m => m.Ignore()); 
        }
    }
}

