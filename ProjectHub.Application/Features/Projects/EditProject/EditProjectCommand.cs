using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos; // ใช้ DTO เดิม

namespace ProjectHub.Application.Features.Projects.EditProject
{
    public class EditProjectCommand : IRequest<ProjectResponseDto>
    {
        [Required]
        public int ProjectId { get; set; } // ID ของ Project ที่จะแก้ไข

        [Required]
        [MaxLength(100)] // อาจจะกำหนด MaxLength
        public string NewName { get; set; } = string.Empty; // ชื่อใหม่
    }
}
