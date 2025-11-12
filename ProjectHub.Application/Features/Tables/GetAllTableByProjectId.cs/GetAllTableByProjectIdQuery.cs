using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Tables.GetAllTablesByProjectId
{
    // "คำสั่งนี้ เมื่อรันแล้ว จะคืนค่ากลับมาเป็น List ของ TableResponseDto"
    public class GetAllTablesByProjectIdQuery : IRequest<IEnumerable<TableResponseDto>>
    {
        // เราต้องมี Property นี้ เพื่อให้ Controller ส่ง ProjectId เข้ามาได้
        public int ProjectId { get; set; }
    }
}