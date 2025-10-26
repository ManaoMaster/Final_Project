using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Users.Register;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Domain -> DTO
            CreateMap<Users, UserResponseDto>()
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.User_id))
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Email))
                .ForMember(d => d.Username, m => m.MapFrom(s => s.Username))
                .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.Created_at));

            // Command -> Domain (ลดการ new ใน Handler)
            CreateMap<RegisterUserCommand, Users>()
                .ForMember(d => d.User_id, m => m.Ignore())          // ให้ DB จัดการ
                .ForMember(d => d.Created_at, m => m.Ignore())          // ให้ default/DB
                .ForMember(d => d.Projects, m => m.Ignore());         // กันวงวน
        }
    }
}
