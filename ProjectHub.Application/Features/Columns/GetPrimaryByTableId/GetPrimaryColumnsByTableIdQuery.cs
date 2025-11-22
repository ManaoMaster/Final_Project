using MediatR;
using ColumnEntity = ProjectHub.Domain.Entities.Columns; 
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Columns.GetPrimaryColumnsByTableId
{
    
    public class GetPrimaryColumnsByTableIdQuery : IRequest<IEnumerable<ColumnEntity>> 
    {
        public int TableId { get; set; }
    }
}