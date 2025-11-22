using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;

using RelationshipEntity = ProjectHub.Domain.Entities.Relationships;

namespace ProjectHub.Application.Features.Relationships.CreateRelationship
{
    public class CreateRelationshipHandler
        : IRequestHandler<CreateRelationshipCommand, RelationshipResponseDto>
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IColumnRepository _columnRepository;
        private readonly IMapper _mapper;

        public CreateRelationshipHandler(
            IRelationshipRepository relationshipRepository,
            IColumnRepository columnRepository,
            IMapper mapper
        )
        {
            _relationshipRepository = relationshipRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
        }

        public async Task<RelationshipResponseDto> Handle(
            CreateRelationshipCommand request,
            CancellationToken cancellationToken
        )
        {
            
            if (request.PrimaryColumnId == request.ForeignColumnId)
            {
                throw new ArgumentException(
                    "Cannot create a relationship from a column to itself."
                );
            }

            var primaryColumn = await _columnRepository.GetColumnByIdAsync(request.PrimaryColumnId);
            var foreignColumn = await _columnRepository.GetColumnByIdAsync(request.ForeignColumnId);

            if (primaryColumn == null)
                throw new ArgumentException(
                    $"Primary Column with ID {request.PrimaryColumnId} not found."
                );
            if (foreignColumn == null)
                throw new ArgumentException(
                    $"Foreign Column with ID {request.ForeignColumnId} not found."
                );

            if (primaryColumn.Table_id != request.PrimaryTableId)
                throw new ArgumentException(
                    "Primary Column does not belong to the specified Primary Table."
                );
            if (foreignColumn.Table_id != request.ForeignTableId)
                throw new ArgumentException(
                    "Foreign Column does not belong to the specified Foreign Table."
                );

            if (request.PrimaryTableId == request.ForeignTableId)
            {
                throw new ArgumentException("Cannot create a relationship within the same table.");
            }

            
            if (primaryColumn.Is_primary == false)
            {
                throw new ArgumentException("Primary Column must be a primary key.");
            }

            if (
                primaryColumn.Data_type?.ToUpperInvariant()
                != foreignColumn.Data_type?.ToUpperInvariant()
            )
            {
                throw new ArgumentException(
                    $"Data type mismatch. Cannot link {primaryColumn.Data_type} (Primary) to {foreignColumn.Data_type} (Foreign)."
                );
            }

            
            
            var relationshipEntity = _mapper.Map<RelationshipEntity>(request);

            
            await _relationshipRepository.AddRelationshipAsync(relationshipEntity);

            
            var responseDto = _mapper.Map<RelationshipResponseDto>(relationshipEntity);

            return responseDto;
        }
    }
}
