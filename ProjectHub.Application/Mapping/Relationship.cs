using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Relationships.CreateRelationship;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping
{
    public class RelationshipProfile : Profile
    {
        public RelationshipProfile()
        {
            // Domain (Entity) -> DTO (Response)
            // (ชื่อ Property ตรงกัน AutoMapper จัดการให้ส่วนใหญ่)
            CreateMap<Relationships, RelationshipResponseDto>();

            // Command -> Domain (Entity)
            CreateMap<CreateRelationshipCommand, Relationships>()
                .ForMember(d => d.RelationshipId, m => m.Ignore()) // Database จะสร้างให้
                // Property อื่นๆ (Id) ชื่อตรงกัน AutoMapper จัดการให้
                // Ignore Navigation Properties
                .ForMember(d => d.PrimaryTable, m => m.Ignore())
                .ForMember(d => d.PrimaryColumn, m => m.Ignore())
                .ForMember(d => d.ForeignTable, m => m.Ignore())
                .ForMember(d => d.ForeignColumn, m => m.Ignore());
        }
    }
}
 