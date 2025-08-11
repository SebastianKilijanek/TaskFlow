using Moq;
using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.Handlers;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Unit.Application.Auth;

public class AuthHandlersTests
{
    [Fact]
    public async Task RegisterUserHandler_Should_Create_User_And_Return_Tokens()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtService = new Mock<IJwtService>();
        var passwordHasher = new PasswordHasher<User>();

        unitOfWork.Setup(r => r.Repository<User>().AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        jwtService.Setup(s => s.GenerateTokens(It.IsAny<User>()))
            .Returns(("access-token", "refresh-token"));

        var handler = new RegisterUserHandler(unitOfWork.Object, jwtService.Object, passwordHasher);
        var command = new RegisterUserCommand("test@test.com", "Tester", "test123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("Tester", result.UserName);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("User", result.Role);
        unitOfWork.Verify(r => r.Repository<User>().AddAsync(
            It.Is<User>(u => u.Email == "test@test.com" && u.UserName == "Tester")), Times.Once);
    }

    [Fact]
    public async Task LoginUserHandler_Should_Return_Tokens_For_Valid_User()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            UserName = "tester",
            PasswordHash = String.Empty,
            Role = UserRole.User
        };
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "Password123");

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(r => r.UserRepository.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(s => s.GenerateTokens(user))
            .Returns(("access-token", "refresh-token"));

        var handler = new LoginUserHandler(unitOfWork.Object, jwtService.Object, passwordHasher);
        var command = new LoginUserCommand("test@email.com", "Password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("tester", result.UserName);
        Assert.Equal("test@email.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task LoginUserHandler_Should_Return_Null_For_Invalid_Password()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            UserName = "tester",
            PasswordHash = String.Empty,
            Role = UserRole.User
        };
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "CorrectPassword");

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(r => r.UserRepository.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var jwtService = new Mock<IJwtService>();

        var handler = new LoginUserHandler(unitOfWork.Object, jwtService.Object, passwordHasher);
        var command = new LoginUserCommand("test@email.com", "WrongPassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginUserHandler_Should_Return_Null_For_NotFound_User()
    {
        // Arrange
        var passwordHasher = new PasswordHasher<User>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(r => r.UserRepository.GetByEmailAsync("notfound@email.com"))
            .ReturnsAsync((User)null!);

        var jwtService = new Mock<IJwtService>();

        var handler = new LoginUserHandler(unitOfWork.Object, jwtService.Object, passwordHasher);
        var command = new LoginUserCommand("notfound@email.com", "Password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_Return_New_Tokens_For_Valid_RefreshToken()
    {
        // Arrange
        var principalMock = new Mock<System.Security.Claims.ClaimsPrincipal>();
        principalMock.SetupGet(p => p.Identity.Name).Returns("test@email.com");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            UserName = "tester",
            PasswordHash = String.Empty,
            Role = UserRole.User
        };
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(r => r.UserRepository.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(s => s.GetPrincipalFromToken("refresh-token", true)).Returns(principalMock.Object);
        jwtService.Setup(s => s.GenerateTokens(user)).Returns(("access-token", "new-refresh-token"));

        var handler = new RefreshTokenHandler(unitOfWork.Object, jwtService.Object);
        var command = new RefreshTokenCommand("refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal("tester", result.UserName);
        Assert.Equal("test@email.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_Return_Null_For_Invalid_RefreshToken()
    {
        // Arrange
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(s => s.GetPrincipalFromToken("invalid-refresh-token", false))
            .Returns((System.Security.Claims.ClaimsPrincipal)null!);

        var handler = new RefreshTokenHandler(unitOfWork.Object, jwtService.Object);
        var command = new RefreshTokenCommand("invalid-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}