using MediatR;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    
    
    
    
    
    
    public class GetRowsByTableIdQuery : IRequest<IEnumerable<IDictionary<string, object>>>
    {
        
        
        
        public int TableId { get; set; }
    }
}