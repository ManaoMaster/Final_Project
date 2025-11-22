using AutoMapper;
using ProjectHub.Application.Dtos; 
using ProjectHub.Application.Features.Rows.CreateRow; 
using ProjectHub.Application.Features.Rows.UpdateRow;
using ProjectHub.Domain.Entities; 

namespace ProjectHub.Application.Mapping
{
    public class RowProfile : Profile
    {
        public RowProfile()
        {
            
            CreateMap<Rows, RowResponseDto>()
                .ForMember(d => d.RowId, m => m.MapFrom(s => s.Row_id))
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at))
                .ForMember(d => d.Data, m => m.MapFrom(s => s.Data)); 

            
            CreateMap<CreateRowCommand, Rows>()
                .ForMember(d => d.Row_id, m => m.Ignore()) 
                .ForMember(d => d.Created_at, m => m.Ignore()) 
                .ForMember(d => d.Table_id, m => m.MapFrom(s => s.TableId)) 
                .ForMember(d => d.Data, m => m.MapFrom(s => s.Data)) 
                
                .ForMember(d => d.Table, m => m.Ignore()); 

            CreateMap<UpdateRowCommand, Rows>()
                .ForMember(d => d.Row_id, m => m.Ignore())
                .ForMember(d => d.Created_at, m => m.Ignore())
                .ForMember(d => d.Table_id, m => m.Ignore())
                .ForMember(d => d.Data, m => m.MapFrom(s => s.NewData));
        }
    }
}
