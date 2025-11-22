using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Features.Relationships.CreateRelationship;
using ProjectHub.Application.Features.Relationships.UpdateRelationship;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping
{
    public class RelationshipProfile : Profile
    {
        public RelationshipProfile()
        {
            
            
            CreateMap<Relationships, RelationshipResponseDto>();

            
            CreateMap<CreateRelationshipCommand, Relationships>()
                .ForMember(d => d.RelationshipId, m => m.Ignore()) 
                                                                   
                                                                   
                .ForMember(d => d.PrimaryTable, m => m.Ignore())
                .ForMember(d => d.PrimaryColumn, m => m.Ignore())
                .ForMember(d => d.ForeignTable, m => m.Ignore())
                .ForMember(d => d.ForeignColumn, m => m.Ignore());
                
            CreateMap<UpdateRelationshipCommand, Relationships>();    
        }
    }
}
 