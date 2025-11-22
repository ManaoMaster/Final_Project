using MediatR;
using ColumnEntity = ProjectHub.Domain.Entities.Columns; 
using System.Collections.Generic;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId
{
    
    
    
    
    

    public class GetColumnsByTableIdQuery
    : IRequest<IEnumerable<ColumnResponseDto>>
    {
        public int TableId { get; set; }
    }



}