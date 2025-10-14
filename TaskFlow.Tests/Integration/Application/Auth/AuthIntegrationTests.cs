using Xunit;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.Tests.Application.Auth;

[Collection("SequentialTests")]
public class AuthIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task RegisterUserHandler_Should_CreateUserAndReturnTokens()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");

        // Act
        var result = await testScope.Mediator.Send(command);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Equal(command.UserName, result.UserName);
        Assert.Equal(command.Email, result.Email);

        var dbUser = await testScope.UnitOfWork.UserRepository.GetByEmailAsync(command.Email);
        Assert.NotNull(dbUser);
        Assert.Equal(command.Email, dbUser.Email);
    }

    [Fact]
    public async Task LoginUserHandler_Should_ReturnTokens_ForExistingUser()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await testScope.Mediator.Send(registerCommand);
        
        var loginCommand = new LoginUserCommand("test@test.com", "Password123!");

        // Act
        var result = await testScope.Mediator.Send(loginCommand);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_ReturnNewTokens()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        var initialResult = await testScope.Mediator.Send(registerCommand);

        var refreshCommand = new RefreshTokenCommand(initialResult.RefreshToken);

        // Act
        var result = await testScope.Mediator.Send(refreshCommand);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.NotEqual(initialResult.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task RegisterUserHandler_Should_ThrowConflictException_WhenEmailExists()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await testScope.Mediator.Send(command);

        // Act 
        // Assert
        await Assert.ThrowsAsync<ConflictException>(() => testScope.Mediator.Send(command));
    }

    [Fact]
    public async Task LoginUserHandler_Should_ThrowUnauthorizedAccessException_ForInvalidPassword()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var registerCommand = new RegisterUserCommand("test@test.com", "Tester", "Password123!");
        await testScope.Mediator.Send(registerCommand);

        var loginCommand = new LoginUserCommand("test@test.com", "WrongPassword!");

        // Act 
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => testScope.Mediator.Send(loginCommand));
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_ThrowUnauthorizedAccessException_ForInvalidToken()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var command = new RefreshTokenCommand("invalid-token");

        // Act 
        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => testScope.Mediator.Send(command));
    }
}