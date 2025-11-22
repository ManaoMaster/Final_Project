using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Tables.CreateTable; 
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Features.Tables.UpdateTable;
using ProjectHub.Domain.Entities; 

namespace ProjectHub.Application.Mapping
{
    public class TableProfile : Profile
    {
        public TableProfile()
        {
            
            CreateMap<Tables, TableResponseDto>()
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id))
                .ForMember(d => d.ProjectId, m => m.MapFrom(s => s.Project_id))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name));

            
            CreateMap<CreateTableCommand, Tables>()
                .ForMember(d => d.Table_id, m => m.Ignore()) 
                .ForMember(d => d.Created_at, m => m.Ignore()) 
                                                               
                .ForMember(d => d.Project_id, m => m.MapFrom(s => s.ProjectId)) 
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                
                .ForMember(d => d.Projects, m => m.Ignore())
                .ForMember(d => d.Columns, m => m.Ignore())
                .ForMember(d => d.Rows, m => m.Ignore());
                

            CreateMap<UpdateTableCommand, Tables>()
                .ForMember(d => d.Name, m => m.MapFrom(s => s.NewName))
                .ForMember(d => d.Table_id, m => m.Ignore())
                .ForMember(d => d.Created_at, m => m.Ignore())
                
                .ForMember(d => d.Projects, m => m.Ignore())
                .ForMember(d => d.Columns, m => m.Ignore())
                .ForMember(d => d.Rows, m => m.Ignore());

        }
    }
}
