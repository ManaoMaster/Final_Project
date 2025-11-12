using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Projects.GetAllProjects
{
    public class GetAllProjectsQuery : IRequest<IEnumerable<ProjectResponseDto>>
    {
        // *** [FIX] ***
        // เราจะให้ Controller ส่ง UserId เข้ามาในนี้
        // แทนที่ Handler จะไปหาเอง
        public int UserId { get; set; }
    }
}