using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using Xunit;

namespace TaskFlow.Tests.API.Auth;

[Collection("SequentialTests")]
public class AuthAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task Register_Should_CreateUserAndReturnTokens()
    {
        // Arrange
        factory.GetTestScope();
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("test@test.com", "Tester", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrEmpty(authResult.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResult.RefreshToken));
        Assert.Equal(command.UserName, authResult.UserName);
        Assert.Equal(command.Email, authResult.Email);
    }

    [Fact]
    public async Task Login_Should_ReturnTokens_ForExistingUser()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClient();
        await TestSeeder.SeedUser(testScope, hashPassword: true);
        var command = new LoginUserCommand("test@test.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrEmpty(authResult.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResult.RefreshToken));
    }

    [Fact]
    public async Task Refresh_Should_ReturnNewTokens()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClient();
        await TestSeeder.SeedUser(testScope, hashPassword: true);
        var loginCommand = new LoginUserCommand("test@test.com", "Password123!");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDTO>();
        
        var command = new RefreshTokenCommand(loginResult!.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrEmpty(authResult.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResult.RefreshToken));
        Assert.NotEqual(loginResult.RefreshToken, authResult.RefreshToken);
    }

    [Fact]
    public async Task Register_Should_ReturnConflict_WhenEmailExists()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClient();
        await TestSeeder.SeedUser(testScope, hashPassword: true);
        var command = new RegisterUserCommand("test@test.com", "OtherUser", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_ReturnUnauthorized_ForInvalidCredentials()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClient();
        await TestSeeder.SeedUser(testScope, hashPassword: true);
        var command = new LoginUserCommand("test@test.com", "WrongPassword!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_Should_ReturnUnauthorized_ForInvalidToken()
    {
        // Arrange
        factory.GetTestScope();
        var client = factory.CreateClient();
        var command = new RefreshTokenCommand("invalid-token");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}