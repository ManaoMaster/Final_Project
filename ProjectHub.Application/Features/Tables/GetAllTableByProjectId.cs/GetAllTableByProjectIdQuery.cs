using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Tables.GetAllTablesByProjectId
{
    
    public class GetAllTablesByProjectIdQuery : IRequest<IEnumerable<TableResponseDto>>
    {
        
        public int ProjectId { get; set; }
    }
}