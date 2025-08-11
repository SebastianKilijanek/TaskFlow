using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Auth;

[Collection("SequentialTests")]
public class AuthAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_And_Login_Should_Return_Tokens()
    {
        // Arrange
        factory.GetTestScope();
        
        // Register
        var registerPayload = new
        {
            email = "test@test.com",
            userName = "Tester",
            password = "test123"
        };
        var regContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");

        // Act
        var regResponse = await _client.PostAsync("/api/auth/register", regContent);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);
        var regBody = await regResponse.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", regBody);
        Assert.Contains("refreshToken", regBody);

        // Login
        // Arrange
        var loginPayload = new
        {
            email = "test@test.com",
            password = "test123"
        };
        var logContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");

        // Act
        var logResponse = await _client.PostAsync("/api/auth/login", logContent);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, logResponse.StatusCode);
        var logBody = await logResponse.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", logBody);
        Assert.Contains("refreshToken", logBody);
    }
    
    [Fact]
    public async Task Refresh_Should_Return_New_Tokens()
    {
        // Arrange
        factory.GetTestScope();
        
        var registerPayload = new
        {
            email = "test@test.com",
            userName = "Tester",
            password = "test123"
        };
        var regContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
        var regResponse = await _client.PostAsync("/api/auth/register", regContent);
        var regBody = await regResponse.Content.ReadAsStringAsync();
        var regJson = JsonDocument.Parse(regBody).RootElement;
        var refreshToken = regJson.GetProperty("refreshToken").GetString();

        var refreshPayload = new { refreshToken };
        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshPayload), Encoding.UTF8, "application/json");
        
        // Act
        var refreshResponse = await _client.PostAsync("/api/auth/refresh", refreshContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshBody = await refreshResponse.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", refreshBody);
        Assert.Contains("refreshToken", refreshBody);
    }
}