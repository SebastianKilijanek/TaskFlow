using MediatR;

namespace TaskFlow.Application.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;