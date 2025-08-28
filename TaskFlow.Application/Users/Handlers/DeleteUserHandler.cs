using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class DeleteUserHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.Repository<User>();

        userRepository.Remove(request.Entity);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}