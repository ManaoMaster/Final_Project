using AutoMapper;
using ProjectHub.Application.Dtos; // ใช้ DTO
using ProjectHub.Application.Features.Rows.CreateRow; // ใช้ Command
using ProjectHub.Domain.Entities; // ใช้ Entity

namespace ProjectHub.Application.Mapping
{
    public class RowProfile : Profile
    {
        public RowProfile()
        {
            // Domain -> DTO
            CreateMap<Rows, RowResponseDto>()
                .ForMember(d => d.RowId, m => m.MapFrom(s => s.Row_id))
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at))
                .ForMember(d => d.Data, m => m.MapFrom(s => s.Data)); // ชื่อตรงกัน แต่ใส่ไว้เพื่อความชัดเจน

            // Command -> Domain (เพื่อใช้ใน Handler)
            CreateMap<CreateRowCommand, Rows>()
                .ForMember(d => d.Row_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Created_at, m => m.Ignore()) // Entity มี Default
                .ForMember(d => d.Table_id, m => m.MapFrom(s => s.TableId)) // ชื่อไม่ตรงกัน
                .ForMember(d => d.Data, m => m.MapFrom(s => s.Data)) // ชื่อตรงกัน
                // *** เพิ่ม: Ignore Navigation Property เพื่อความชัดเจน ***
                .ForMember(d => d.Table, m => m.Ignore()); // Ignore Navigation Property 'Table'
        }
    }
}

