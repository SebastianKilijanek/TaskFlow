using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Application.Users.DTO;
using Xunit;

namespace TaskFlow.Tests.API.Auth;

[Collection("SequentialTests")]
public class RefreshTokenTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private static async Task<AuthResultDTO> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginUserCommand("test@test.com", "Password123!"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResultDTO>())!;
    }

    [Fact]
    public async Task Refresh_OldTokenRejected_AfterRotation()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, hashPassword: true);
        var client = factory.CreateClient();
        var login = await LoginAsync(client);

        var first = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task Refresh_RevokedChain_Returns401()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, hashPassword: true);
        var client = factory.CreateClient();
        var login = await LoginAsync(client);

        var refresh1 = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refresh1.StatusCode);
        var tokens1 = await refresh1.Content.ReadFromJsonAsync<AuthResultDTO>();

        var refresh2 = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(tokens1!.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refresh2.StatusCode);

        var reuse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(tokens1.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, hashPassword: true);
        var client = factory.CreateClient();
        var login = await LoginAsync(client);

        var logout = await client.PostAsJsonAsync("/api/auth/logout",
            new LogoutCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_InvalidatesRefreshTokens()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, hashPassword: true);
        var loginClient = factory.CreateClient();
        var login = await LoginAsync(loginClient);

        var authClient = factory.CreateClientWithClaims();
        var change = await authClient.PutAsJsonAsync("/api/users/me/change-password",
            new ChangeUserPasswordDTO { CurrentPassword = "Password123!", NewPassword = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);

        var refresh = await loginClient.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenCommand(login.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}