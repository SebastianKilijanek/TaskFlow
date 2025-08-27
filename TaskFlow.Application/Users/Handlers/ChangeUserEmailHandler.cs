using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class ChangeUserEmailHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeUserEmailCommand, Unit>
{
    public async Task<Unit> Handle(ChangeUserEmailCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.Repository<User>();

        var existingUserWithEmail = await unitOfWork.UserRepository.GetByEmailAsync(request.NewEmail);
        if (existingUserWithEmail is not null && existingUserWithEmail.Id != request.UserId)
        {
            throw new ConflictException($"Email {request.NewEmail} is already in use.");
        }

        request.User.Email = request.NewEmail;
        userRepository.Update(request.User);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}