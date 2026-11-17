using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Application.Users.DTO;
using Xunit;

namespace TaskFlow.Tests.API.Validation;

[Collection("SequentialTests")]
public class ValidationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task Register_EmptyEmail_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("", "Tester", "Password123!");

        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("test@test.com", "Tester", "123");

        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBoard_TooLongName_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var payload = new CreateBoardDTO { Name = new string('a', 101), IsPublic = false };

        var response = await client.PostAsJsonAsync("/api/v1/boards", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateColumn_EmptyName_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var payload = new CreateColumnDTO { Name = "", BoardId = Guid.NewGuid() };

        var response = await client.PostAsJsonAsync("/api/columns", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTaskItem_EmptyTitle_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var payload = new CreateTaskItemDTO { Title = "", Description = string.Empty, ColumnId = Guid.NewGuid() };

        var response = await client.PostAsJsonAsync("/api/taskitems", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_SamePassword_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var payload = new ChangeUserPasswordDTO { CurrentPassword = "Password123!", NewPassword = "Password123!" };

        var response = await client.PutAsJsonAsync("/api/users/me/change-password", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveTaskItem_NegativePosition_Returns400()
    {
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var payload = new MoveTaskItemDTO { NewColumnId = Guid.NewGuid(), NewPosition = -1 };

        var response = await client.PostAsJsonAsync($"/api/taskitems/{Guid.NewGuid()}/move", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}