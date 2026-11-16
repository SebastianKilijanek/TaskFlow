using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Common.Interfaces;
using Xunit;

namespace TaskFlow.Tests.API.Auth;

[Collection("SequentialTests")]
public class JwtAuthenticationTests(JwtWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<JwtWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task NoToken_Returns401()
    {
        factory.GetTestScope();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/boards");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InvalidToken_Returns401()
    {
        factory.GetTestScope();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-token");

        var response = await client.GetAsync("/api/v1/boards");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ValidToken_Returns200()
    {
        var scope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(scope, hashPassword: true);
        var jwtService = scope.ServiceScope.ServiceProvider.GetRequiredService<IJwtService>();
        var (accessToken, _) = jwtService.GenerateTokens(user);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/v1/boards");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}