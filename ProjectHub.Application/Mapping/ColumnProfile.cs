using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn; 
using ProjectHub.Application.Features.Columns.DeleteColumn;
using ProjectHub.Application.Features.Columns.UpdateColumn;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Domain.Entities; 

namespace ProjectHub.Application.Mapping
{
    public class ColumnProfile : Profile
    {
        public ColumnProfile()
        {
            
            CreateMap<Columns, ColumnResponseDto>()
                .ForMember(d => d.ColumnId, m => m.MapFrom(s => s.Column_id)) 
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id)) 
                .ForMember(d => d.DataType, m => m.MapFrom(s => s.Data_type)) 
                .ForMember(d => d.IsPrimary, m => m.MapFrom(s => s.Is_primary)) 
                .ForMember(d => d.IsNullable, m => m.MapFrom(s => s.Is_nullable)) 
                .ForMember(d => d.FormulaDefinition, m => m.MapFrom(s => s.FormulaDefinition))
                
                
                .ForMember(d => d.PrimaryKeyType, m => m.MapFrom(s => s.PrimaryKeyType))
                
                .ForMember(d => d.LookupRelationshipId, m => m.MapFrom(s => s.LookupRelationshipId))
                .ForMember(d => d.LookupTargetColumnId, m => m.MapFrom(s => s.LookupTargetColumnId))
                
                .ForMember(
                    d => d.LookupTargetTableId,
                    m =>
                        m.MapFrom(s =>
                            s.LookupRelationship != null
                                ? (int?)s.LookupRelationship.PrimaryTableId
                                : null
                        )
                )
                
                .ForMember(
                    d => d.LookupTargetColumnName,
                    m =>
                        m.MapFrom(s =>
                            s.LookupRelationship != null
                            && s.LookupRelationship.PrimaryColumn != null
                                ? s.LookupRelationship.PrimaryColumn.Name
                                : null
                        )
                );

            

            
            CreateMap<CreateColumnCommand, Columns>()
                .ForMember(d => d.Column_id, m => m.Ignore()) 
                .ForMember(d => d.Table_id, m => m.MapFrom(s => s.TableId)) 
                .ForMember(d => d.Data_type, m => m.MapFrom(s => s.DataType)) 
                .ForMember(d => d.Is_primary, m => m.MapFrom(s => s.IsPrimary)) 
                .ForMember(d => d.Is_nullable, m => m.MapFrom(s => s.IsNullable)) 
                .ForMember(d => d.FormulaDefinition, m => m.MapFrom(s => s.FormulaDefinition))
                .ForMember(d => d.LookupRelationshipId, m => m.MapFrom(s => s.LookupRelationshipId))
                .ForMember(d => d.LookupTargetColumnId, m => m.MapFrom(s => s.LookupTargetColumnId))
                
                
                .ForMember(d => d.Tables, m => m.Ignore());
            CreateMap<UpdateColumnCommand, Columns>()
                
                .ForMember(d => d.Column_id, m => m.Ignore())
                .ForMember(d => d.Table_id, m => m.Ignore())
                
                .ForMember(d => d.Name, m => m.MapFrom(s => s.NewName))
                .ForMember(d => d.Data_type, m => m.MapFrom(s => s.NewDataType))
                .ForMember(d => d.Is_primary, m => m.MapFrom(s => s.NewIsPrimary))
                .ForMember(d => d.Is_nullable, m => m.MapFrom(s => s.NewIsNullable));
        }
    }
}
