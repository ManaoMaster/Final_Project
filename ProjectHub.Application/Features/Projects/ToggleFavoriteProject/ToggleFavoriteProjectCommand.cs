using MediatR;
using ProjectHub.Application.Dtos; // (เราจะคืนค่า Project DTO ที่อัปเดตแล้วกลับไป)

namespace ProjectHub.Application.Features.Projects.ToggleFavoriteProject
{
    // นี่คือ "คำสั่ง"
    // IRequest<ProjectResponseDto>
    // หมายความว่า: "คำสั่งนี้ เมื่อรันแล้ว จะคืนค่า Project ที่อัปเดตแล้วกลับไป"
    public class ToggleFavoriteProjectCommand : IRequest<ProjectResponseDto>
    {
        // ID ของ Project ที่จะกดดาว
        public int ProjectId { get; set; }

        // ID ของ User ที่เป็นคนกด (Controller จะหาค่านี้มาจาก Token)
        public int UserId { get; set; }
    }
}