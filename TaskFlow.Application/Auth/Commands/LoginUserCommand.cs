using MediatR;
using TaskFlow.Application.Auth.DTO;

namespace TaskFlow.Application.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResultDTO>;