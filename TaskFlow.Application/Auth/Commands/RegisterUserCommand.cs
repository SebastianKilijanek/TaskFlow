using MediatR;
using TaskFlow.Application.Auth.DTO;

namespace TaskFlow.Application.Auth.Commands;

public record RegisterUserCommand(string Email, string UserName, string Password) : IRequest<AuthResultDTO>;