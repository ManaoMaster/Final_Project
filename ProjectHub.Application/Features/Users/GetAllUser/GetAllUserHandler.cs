using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllUsersHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<UserResponseDto>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            // ดึงลิสต์ Users (entity) จาก repo
            var users = await _userRepository.GetAllAsync();

            //  map เป็นลิสต์ DTO
            var dtoList = _mapper.Map<List<UserResponseDto>>(users);

            return dtoList;
        }
    }
}
