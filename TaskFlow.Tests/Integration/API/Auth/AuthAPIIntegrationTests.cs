using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Auth;

[Collection("SequentialTests")]
public class AuthAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private static async Task<AuthResultDTO?> RegisterUser(HttpClient client, string email, string password)
    {
        var command = new RegisterUserCommand(email, "Tester", password);
        var response = await client.PostAsJsonAsync("/api/auth/register", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResultDTO>();
    }
    
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
        factory.GetTestScope();
        var client = factory.CreateClient();
        await RegisterUser(client, "test@test.com", "Password123!");
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
        factory.GetTestScope();
        var client = factory.CreateClient();
        var registrationResult = await RegisterUser(client, "test@test.com", "Password123!");
        var command = new RefreshTokenCommand(registrationResult!.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var authResult = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrEmpty(authResult.AccessToken));
        Assert.False(string.IsNullOrEmpty(authResult.RefreshToken));
        Assert.NotEqual(registrationResult.RefreshToken, authResult.RefreshToken);
    }

    [Fact]
    public async Task Register_Should_ReturnConflict_WhenEmailExists()
    {
        // Arrange
        factory.GetTestScope();
        var client = factory.CreateClient();
        var email = "test@test.com";
        await RegisterUser(client, email, "Password123!");
        var command = new RegisterUserCommand(email, "AnotherUser", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_ReturnUnauthorized_ForInvalidCredentials()
    {
        // Arrange
        factory.GetTestScope();
        var client = factory.CreateClient();
        await RegisterUser(client, "test@test.com", "Password123!");
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