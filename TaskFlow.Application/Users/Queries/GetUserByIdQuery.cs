using MediatR;
using TaskFlow.Application.Users.DTO;

namespace TaskFlow.Application.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDTO?>;