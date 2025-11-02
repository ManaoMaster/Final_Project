using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Columns.CreateColumn; // Command
using ProjectHub.Application.Features.Columns.DeleteColumn;
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
                .ForMember(d => d.IsNullable, m => m.MapFrom(s => s.Is_nullable)); // ชื่อไม่ตรง
            // Name ชื่อตรงกัน ไม่ต้องเขียน

            // Command -> Domain (Entity) (ใช้ d, s)
            CreateMap<CreateColumnCommand, Columns>()
                .ForMember(d => d.Column_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Table_id, m => m.MapFrom(s => s.TableId)) // ชื่อไม่ตรง
                .ForMember(d => d.Data_type, m => m.MapFrom(s => s.DataType)) // ชื่อไม่ตรง
                .ForMember(d => d.Is_primary, m => m.MapFrom(s => s.IsPrimary)) // ชื่อไม่ตรง
                .ForMember(d => d.Is_nullable, m => m.MapFrom(s => s.IsNullable)) // ชื่อไม่ตรง
                .ForMember(d => d.FormulaDefinition, m => m.MapFrom(s => s.FormulaDefinition))
                // Name ชื่อตรงกัน ไม่ต้องเขียน
                // Ignore Navigation Property
                .ForMember(d => d.Tables, m => m.Ignore());
            CreateMap<UpdateColumnCommand, Columns>()
                .ForMember(d => d.Column_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Table_id, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Data_type, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Is_primary, m => m.Ignore()) // Database จะสร้างให้
                .ForMember(d => d.Is_nullable, m => m.Ignore()); // Database จะสร้างให้
            // Name ชื่อตรงกัน ไม่ต้องเขียน

        }
    }
}
