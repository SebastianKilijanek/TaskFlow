using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Application.Users.Handlers;
using Xunit;
using TaskFlow.Application.Users.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Unit.Application.Users;

public class UsersHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<User>> _userRepositoryMock = new();
    private readonly Mock<IUserRepository> _domainUserRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new();

    public UsersHandlersTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_domainUserRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUsersHandler_Should_ReturnMappedUsers()
    {
        // Arrange
        var users = new List<User> { new() { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" } };
        var userDtos = new List<UserDTO> { new() { Id = users[0].Id, Email = users[0].Email, UserName = users[0].UserName, Role = UserRole.User } };

        _userRepositoryMock.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(users);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<UserDTO>>(users)).Returns(userDtos);

        var handler = new GetUsersHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userDtos, result);
    }

    [Fact]
    public async Task GetUserByIdHandler_Should_ReturnUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var userDto = new UserDTO { Id = user.Id, Email = user.Email, UserName = user.UserName, Role = UserRole.User };

        _mapperMock.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

        var handler = new GetUserByIdHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetUserByIdQuery(user.Id) { Entity = user };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userDto, result);
    }

    [Fact]
    public async Task UpdateUserHandler_Should_UpdateUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash", Role = UserRole.User };
        var handler = new UpdateUserHandler(_unitOfWorkMock.Object);
        var command = new UpdateUserCommand(user.Id, "newtest@test.com", "NewTester", "Admin") { Entity = user };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.Update(It.Is<User>(u =>
            u.Id == user.Id &&
            u.Email == "newtest@test.com" &&
            u.UserName == "NewTester" &&
            u.Role == UserRole.Admin
        )), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserHandler_Should_DeleteUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var handler = new DeleteUserHandler(_unitOfWorkMock.Object);
        var command = new DeleteUserCommand(user.Id) { Entity = user };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.Remove(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeUserPasswordHandler_Should_ChangePassword()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "old_hash" };
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, "current_password"))
            .Returns(PasswordVerificationResult.Success);
        _passwordHasherMock.Setup(p => p.HashPassword(user, "new_password")).Returns("new_hash");

        var handler = new ChangeUserPasswordHandler(_unitOfWorkMock.Object, _passwordHasherMock.Object);
        var command = new ChangeUserPasswordCommand(user.Id, "current_password", "new_password") { User = user };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("new_hash", user.PasswordHash);
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeUserEmailHandler_Should_ChangeEmail()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var newEmail = "newtest@test.com";
        _domainUserRepositoryMock.Setup(r => r.GetByEmailAsync(newEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var handler = new ChangeUserEmailHandler(_unitOfWorkMock.Object);
        var command = new ChangeUserEmailCommand(user.Id, newEmail) { User = user };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newEmail, user.Email);
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeUserEmailHandler_Should_ThrowConflictException_WhenEmailExists()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var existingUser = new User { Id = Guid.NewGuid(), Email = "othertest@test.com", UserName = "OtherTester", PasswordHash = "hash" };
        var newEmail = "othertest@test.com";
        _domainUserRepositoryMock.Setup(r => r.GetByEmailAsync(newEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new ChangeUserEmailHandler(_unitOfWorkMock.Object);
        var command = new ChangeUserEmailCommand(user.Id, newEmail) { User = user };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }
}