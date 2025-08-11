using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.Handlers;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Integration.Application.Auth;

[Collection("SequentialTests")]
public class AuthIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
   [Fact]
    public async Task RegisterUserHandler_And_LoginUserHandler_Should_Work()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var jwt = testScope.ServiceScope.ServiceProvider.GetRequiredService<IJwtService>();
        var hasher = testScope.ServiceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Act
        var registerHandler = new RegisterUserHandler(testScope.UnitOfWork, jwt, hasher);
        var registerResult = await registerHandler.Handle(
            new RegisterUserCommand("test@test.com", "Tester", "test123"), default);

        // Assert
        Assert.Equal("Tester", registerResult.UserName);
        Assert.NotNull(registerResult);

        // Act
        var loginHandler = new LoginUserHandler(testScope.UnitOfWork, jwt, hasher);
        var loginResult = await loginHandler.Handle(
            new LoginUserCommand("test@test.com", "test123"), default);

        // Assert
        Assert.NotNull(loginResult);
        Assert.Equal("Tester", loginResult.UserName);
        Assert.Equal("test@test.com", loginResult.Email);
    }

    [Fact]
    public async Task RefreshTokenHandler_Should_Work()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var jwt = testScope.ServiceScope.ServiceProvider.GetRequiredService<IJwtService>();
        var hasher = testScope.ServiceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var registerHandler = new RegisterUserHandler(testScope.UnitOfWork, jwt, hasher);
        var registerResult = await registerHandler.Handle(
            new RegisterUserCommand("test2@test.com", "Tester2", "test123"), default);

        var loginHandler = new LoginUserHandler(testScope.UnitOfWork, jwt, hasher);
        var loginResult = await loginHandler.Handle(
            new LoginUserCommand("test2@test.com", "test123"), default);

        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrEmpty(loginResult.RefreshToken));
        
        // Act
        var refreshHandler = new RefreshTokenHandler(testScope.UnitOfWork, jwt);
        var refreshResult = await refreshHandler.Handle(
            new RefreshTokenCommand(loginResult.RefreshToken!), default);

        // Assert
        Assert.NotNull(refreshResult);
        Assert.False(string.IsNullOrEmpty(refreshResult.AccessToken));
        Assert.Equal("Tester2", refreshResult.UserName);
        Assert.Equal("test2@test.com", refreshResult.Email);
    }
}