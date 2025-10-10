using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Boards;

[Collection("SequentialTests")]
public class BoardsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string BASE_URL = "/api/v1/boards/";

    private static async Task CreateTestUser(TestScope testScope)
    {
        var userId = Guid.Parse(TestAuthHandler.UserId);
        if (!await testScope.DbContext.Users.AnyAsync(u => u.Id == userId))
        {
            testScope.DbContext.Users.Add(new User { Id = userId, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" });
            await testScope.DbContext.SaveChangesAsync();
        }
    }

    private static async Task<Guid> CreateTestBoard(HttpClient client, string name, bool isPublic)
    {
        var payload = new CreateBoardDTO { Name = name, IsPublic = isPublic };
        var response = await client.PostAsJsonAsync(BASE_URL, payload);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location;
        Assert.NotNull(location);
        return Guid.Parse(location.Segments.Last());
    }

    [Fact]
    public async Task Post_CreateBoard_Returns201AndCreatesUserBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await CreateTestUser(testScope);
        var boardName = "API Test Board";
        var payload = new CreateBoardDTO { Name = boardName, IsPublic = true };
        var ownerId = Guid.Parse(TestAuthHandler.UserId);

        // Act
        var response = await client.PostAsJsonAsync(BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var boardId = Guid.Parse(location.Segments.Last());

        var board = await testScope.DbContext.Boards.FindAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal(boardName, board.Name);

        var userBoard = await testScope.DbContext.UserBoards
            .FirstOrDefaultAsync(ub => ub.BoardId == boardId && ub.UserId == ownerId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Owner, userBoard.BoardRole);
    }

    [Fact]
    public async Task Get_Boards_Returns200WithContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await CreateTestUser(testScope);
        await CreateTestBoard(client, "Board 1", true);
        await CreateTestBoard(client, "Board 2", false);

        // Act
        var response = await client.GetAsync(BASE_URL);

        // Assert
        response.EnsureSuccessStatusCode();
        var boards = await response.Content.ReadFromJsonAsync<List<BoardDTO>>();
        Assert.NotNull(boards);
        Assert.Equal(2, boards.Count);
        Assert.Contains(boards, b => b.Name == "Board 1");
        Assert.Contains(boards, b => b.Name == "Board 2");
    }

    [Fact]
    public async Task Get_BoardById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await CreateTestUser(testScope);
        var createdBoardId = await CreateTestBoard(client, "Get By Id Test", true);

        // Act
        var response = await client.GetAsync($"{BASE_URL}{createdBoardId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var board = await response.Content.ReadFromJsonAsync<BoardDTO>();
        Assert.NotNull(board);
        Assert.Equal("Get By Id Test", board.Name);
    }

    [Fact]
    public async Task Put_UpdateBoard_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await CreateTestUser(testScope);
        var createdBoardId = await CreateTestBoard(client, "Before Update", true);
        var updatePayload = new UpdateBoardDTO { Name = "After Update", IsPublic = false };

        // Act
        var response = await client.PutAsJsonAsync($"{BASE_URL}{createdBoardId}", updatePayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedBoard = await testScope.DbContext.Boards.FindAsync(createdBoardId);
        Assert.NotNull(updatedBoard);
        Assert.Equal("After Update", updatedBoard.Name);
        Assert.False(updatedBoard.IsPublic);
    }

    [Fact]
    public async Task Delete_Board_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await CreateTestUser(testScope);
        var createdBoardId = await CreateTestBoard(client, "To Be Deleted", true);
        
        // Act
        var response = await client.DeleteAsync($"{BASE_URL}{createdBoardId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedBoard = await testScope.DbContext.Boards.FindAsync(createdBoardId);
        Assert.Null(deletedBoard);
    }
}