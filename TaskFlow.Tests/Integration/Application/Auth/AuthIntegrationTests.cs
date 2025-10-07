using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.Tests.Integration.Application.Auth;

[Collection("SequentialTests")]
public class AuthIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task RegisterUserHandler_Should_CreateUserAndReturnTokens()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Equal(command.UserName, result.UserName);
        Assert.Equal(command.Email, result.Email);

        var dbUser = await scope.UnitOfWork.UserRepository.GetByEmailAsync(command.Email);
        Assert.NotNull(dbUser);
        Assert.Equal(command.Email, dbUser.Email);
    }

    [Fact]
    public async Task LoginUserHandler_Should_ReturnTokens_ForExistingUser()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await mediator.Send(registerCommand);
        
        var loginCommand = new LoginUserCommand("test@test.com", "Password123!");

        // Act
        var result = await mediator.Send(loginCommand);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_ReturnNewTokens()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        var initialResult = await mediator.Send(registerCommand);

        var refreshCommand = new RefreshTokenCommand(initialResult.RefreshToken);

        // Act
        var result = await mediator.Send(refreshCommand);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.NotEqual(initialResult.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task RegisterUserHandler_Should_ThrowConflictException_WhenEmailExists()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await mediator.Send(command);

        // Act 
        // Assert
        await Assert.ThrowsAsync<ConflictException>(() => mediator.Send(command));
    }

    [Fact]
    public async Task LoginUserHandler_Should_ThrowUnauthorizedAccessException_ForInvalidPassword()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await mediator.Send(registerCommand);

        var loginCommand = new LoginUserCommand("test@test.com", "WrongPassword!");

        // Act 
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => mediator.Send(loginCommand));
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_ThrowUnauthorizedAccessException_ForInvalidToken()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new RefreshTokenCommand("invalid-token");

        // Act 
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => mediator.Send(command));
    }
}