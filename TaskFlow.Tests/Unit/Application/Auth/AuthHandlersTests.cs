using System.Security.Claims;
using Moq;
using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.Handlers;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Unit.Application.Auth;

public class AuthHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new();
    private readonly Mock<IRepository<User>> _userRepositoryMock = new();

    [Fact]
    public async Task RegisterUserHandler_Should_CreateUserAndReturnTokens()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);
        _unitOfWorkMock.Setup(uow => uow.Repository<User>()).Returns(_userRepositoryMock.Object);
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
            .Returns("hash");
        _jwtServiceMock.Setup(s => s.GenerateTokens(It.IsAny<User>()))
            .Returns(("access-token", "refresh-token"));

        var handler = new RegisterUserHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(command.UserName, result.UserName);
        Assert.Equal(command.Email, result.Email);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserHandler_Should_ThrowConflictException_WhenEmailExists()
    {
        // Arrange
        var existingUser = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Existing", PasswordHash = "hash", Role = UserRole.User };
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync(existingUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new RegisterUserHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
        var command = new RegisterUserCommand(existingUser.Email, "Tester", "Password123!");

        // Act
        // Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserHandler_Should_ReturnTokens_ForValidUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash", Role = UserRole.User };
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, "Password123"))
            .Returns(PasswordVerificationResult.Success);
        _jwtServiceMock.Setup(s => s.GenerateTokens(user))
            .Returns(("access-token", "refresh-token"));

        var handler = new LoginUserHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginUserCommand(user.Email, "Password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task LoginUserHandler_Should_ThrowUnauthorizedAccessException_ForInvalidPassword()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash", Role = UserRole.User };
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, "WrongPassword"))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new LoginUserHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginUserCommand(user.Email, "WrongPassword");

        // Act
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserHandler_Should_ThrowUnauthorizedAccessException_ForNotFoundUser()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var handler = new LoginUserHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginUserCommand("test@test.com", "Password123");

        // Act
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_ReturnNewTokens_ForValidRefreshToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash", Role = UserRole.User };
        var claims = new[]
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _jwtServiceMock.Setup(s => s.GetPrincipalFromToken("valid-refresh-token", true)).Returns(principal);
        _unitOfWorkMock.Setup(uow => uow.UserRepository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(s => s.GenerateTokens(user))
            .Returns(("new-access-token", "new-refresh-token"));

        var handler = new RefreshTokenHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object);
        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
    }
    
    [Fact]
    public async Task RefreshTokenHandler_Should_ThrowUnauthorizedAccessException_ForInvalidRefreshToken()
    {
        // Arrange
        _jwtServiceMock.Setup(s => s.GetPrincipalFromToken("invalid-token", true))
            .Returns((ClaimsPrincipal)null!);

        var handler = new RefreshTokenHandler(_unitOfWorkMock.Object, _jwtServiceMock.Object);
        var command = new RefreshTokenCommand("invalid-token");

        // Act
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}