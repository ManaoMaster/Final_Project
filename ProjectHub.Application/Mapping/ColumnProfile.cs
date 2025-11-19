using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn; // Command
using ProjectHub.Application.Features.Columns.DeleteColumn;
using ProjectHub.Application.Features.Columns.UpdateColumn;
using ProjectHub.Application.Features.Projects.UpdateProject;
using ProjectHub.Domain.Entities; // Entity

namespace ProjectHub.Application.Mapping
{
    public class ColumnProfile : Profile
    {
        public ColumnProfile()
        {
            // Domain (Entity) -> DTO (ใช้ d, s)
            CreateMap<Columns, ColumnResponseDto>()
                .ForMember(d => d.ColumnId, m => m.MapFrom(s => s.Column_id)) // ชื่อไม่ตรง
                .ForMember(d => d.TableId, m => m.MapFrom(s => s.Table_id)) // ชื่อไม่ตรง
                .ForMember(d => d.DataType, m => m.MapFrom(s => s.Data_type)) // ชื่อไม่ตรง
                .ForMember(d => d.IsPrimary, m => m.MapFrom(s => s.Is_primary)) // ชื่อไม่ตรง
                .ForMember(d => d.IsNullable, m => m.MapFrom(s => s.Is_nullable)) // ชื่อไม่ตรง
                 .ForMember(d => d.FormulaDefinition,
                       m => m.MapFrom(s => s.FormulaDefinition))
            //Add new
            // PK meta อยู่บน Columns ไม่ได้อยู่บน Tables
            .ForMember(d => d.PrimaryKeyType,
                       m => m.MapFrom(s => s.PrimaryKeyType))

            // Lookup meta ที่เก็บใน Columns ตรง ๆ
            .ForMember(d => d.LookupRelationshipId,
                       m => m.MapFrom(s => s.LookupRelationshipId))
            .ForMember(d => d.LookupTargetColumnId,
                       m => m.MapFrom(s => s.LookupTargetColumnId))

    //  ตารางปลายทาง = PrimaryTable ของ relationship (master table)
    .ForMember(d => d.LookupTargetTableId,
               m => m.MapFrom(s =>
                   s.LookupRelationship != null
                     ? (int?)s.LookupRelationship.PrimaryTableId
                     : null))

    //  ชื่อคอลัมน์ปลายทาง (เอาไว้โชว์ได้ในอนาคต)
    .ForMember(d => d.LookupTargetColumnName,
               m => m.MapFrom(s =>
                   s.LookupRelationship != null &&
                   s.LookupRelationship.PrimaryColumn != null
                     ? s.LookupRelationship.PrimaryColumn.Name
                     : null));

            // Name ชื่อตรงกัน ไม่ต้องเขียน

            // Command -> Domain (Entity) (ใช้ d, s)
            CreateMap<CreateColumnCommand, Columns>()
                .ForMember(d => d.Column_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Table_id, m => m.MapFrom(s => s.TableId)) // ชื่อไม่ตรง
                .ForMember(d => d.Data_type, m => m.MapFrom(s => s.DataType)) // ชื่อไม่ตรง
                .ForMember(d => d.Is_primary, m => m.MapFrom(s => s.IsPrimary)) // ชื่อไม่ตรง
                .ForMember(d => d.Is_nullable, m => m.MapFrom(s => s.IsNullable)) // ชื่อไม่ตรง
                .ForMember(d => d.FormulaDefinition, m => m.MapFrom(s => s.FormulaDefinition))
                .ForMember(d => d.LookupRelationshipId, m => m.MapFrom(s => s.LookupRelationshipId))
                .ForMember(d => d.LookupTargetColumnId, m => m.MapFrom(s => s.LookupTargetColumnId))
                // Name ชื่อตรงกัน ไม่ต้องเขียน
                // Ignore Navigation Property
                .ForMember(d => d.Tables, m => m.Ignore());
            CreateMap<UpdateColumnCommand, Columns>()
    // ไม่ให้ไปยุ่งกับ id / table_id
    .ForMember(d => d.Column_id, m => m.Ignore())
    .ForMember(d => d.Table_id, m => m.Ignore())

    //  map ค่าจริงจาก Command ลง Entity
    .ForMember(d => d.Name, m => m.MapFrom(s => s.NewName))
    .ForMember(d => d.Data_type, m => m.MapFrom(s => s.NewDataType))
    .ForMember(d => d.Is_primary, m => m.MapFrom(s => s.NewIsPrimary))
    .ForMember(d => d.Is_nullable, m => m.MapFrom(s => s.NewIsNullable));

            

        }
    }
}
