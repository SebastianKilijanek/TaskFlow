using AutoMapper;
using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Application.Users.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class GetUserByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDTO>
{
    public async Task<UserDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return mapper.Map<UserDTO>(request.User);
    }
}