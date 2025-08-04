using MediatR;
using TaskFlow.Application.Auth.DTO;

namespace TaskFlow.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDTO?>;