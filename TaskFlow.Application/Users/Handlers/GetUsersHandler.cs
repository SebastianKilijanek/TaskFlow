using AutoMapper;
using MediatR;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Application.Users.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class GetUsersHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUsersQuery, IEnumerable<UserDTO>>
{
    public async Task<IEnumerable<UserDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Repository<User>().ListAsync();
        return users.Select(mapper.Map<UserDTO>);
    }
}