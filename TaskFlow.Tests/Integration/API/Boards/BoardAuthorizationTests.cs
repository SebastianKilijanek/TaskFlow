using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.API.Boards;

[Collection("SequentialTests")]
public class BoardAuthorizationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string BASE_URL = "/api/v1/boards/";

    [Fact]
    public async Task NonMember_CanReadPublicBoard()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var nonMemberId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, nonMemberId, "outsider@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, name: "Public board", isPublic: true);

        var client = factory.CreateClientForUser(nonMemberId);
        var response = await client.GetAsync($"{BASE_URL}{boardId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task NonMember_CannotUpdatePublicBoard()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var nonMemberId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, nonMemberId, "outsider@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, name: "Public board", isPublic: true);

        var client = factory.CreateClientForUser(nonMemberId);
        var response = await client.PutAsJsonAsync($"{BASE_URL}{boardId}",
            new UpdateBoardDTO { Name = "Hijacked", IsPublic = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        scope.DbContext.ChangeTracker.Clear();
        var board = await scope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId);
        Assert.Equal("Public board", board!.Name);
    }

    [Fact]
    public async Task NonMember_CannotDeletePublicBoard()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var nonMemberId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, nonMemberId, "outsider@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, name: "Public board", isPublic: true);

        var client = factory.CreateClientForUser(nonMemberId);
        var response = await client.DeleteAsync($"{BASE_URL}{boardId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        scope.DbContext.ChangeTracker.Clear();
        Assert.NotNull(await scope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId));
    }

    [Fact]
    public async Task Viewer_CannotUpdatePublicBoard()
    {
        var scope = factory.GetTestScope();
        var ownerId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, ownerId, "owner@test.com");
        var viewerId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, viewerId, "viewer@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, ownerId: ownerId, name: "Public board", isPublic: true);
        await TestSeeder.SeedUserBoard(scope, viewerId, boardId, BoardRole.Viewer);

        var client = factory.CreateClientForUser(viewerId);
        var response = await client.PutAsJsonAsync($"{BASE_URL}{boardId}",
            new UpdateBoardDTO { Name = "Hijacked", IsPublic = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NonMember_CannotReadPrivateBoard()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var nonMemberId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, nonMemberId, "outsider@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, name: "Private board", isPublic: false);

        var client = factory.CreateClientForUser(nonMemberId);
        var response = await client.GetAsync($"{BASE_URL}{boardId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}