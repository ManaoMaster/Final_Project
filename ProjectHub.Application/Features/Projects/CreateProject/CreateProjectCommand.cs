using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Projects.CreateProject;

public class CreateProjectCommand : IRequest<ProjectResponseDto>
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
