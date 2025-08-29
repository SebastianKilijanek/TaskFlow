using AutoMapper;
using MediatR;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Application.Users.Queries;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class GetUserByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDTO>
{
    public Task<UserDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(mapper.Map<UserDTO>(request.Entity));
    }
}