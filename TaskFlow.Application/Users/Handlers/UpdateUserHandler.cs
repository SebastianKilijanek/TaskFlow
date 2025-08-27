using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class UpdateUserHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Unit>
{
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.Repository<User>();
        
        if (request.User.Email != request.Email)
        {
            var existingUserWithEmail = await unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != request.UserId)
            {
                throw new ConflictException($"Email '{request.Email}' is already in use.");
            }
            request.User.Email = request.Email;
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new BadRequestException($"Invalid role: '{request.Role}'.");
        }

        request.User.UserName = request.UserName;
        request.User.Role = userRole;

        userRepository.Update(request.User);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}