using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.API.Boards;

[Collection("SequentialTests")]
public class BoardsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string BASE_URL = "/api/v1/boards/";

    [Fact]
    public async Task Post_CreateBoard_Returns201AndCreatesUserBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var ownerId = (await TestSeeder.SeedUser(testScope)).Id;
        var boardName = "API Test Board";
        var payload = new CreateBoardDTO { Name = boardName, IsPublic = true };

        // Act
        var response = await client.PostAsJsonAsync(BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var boardId = Guid.Parse(location.Segments.Last());

        var board = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal(boardName, board.Name);

        var userBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(ownerId, boardId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Owner, userBoard.BoardRole);
    }

    [Fact]
    public async Task Get_Boards_Returns200WithContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await TestSeeder.SeedUser(testScope);
        await TestSeeder.SeedBoard(testScope, name: "Board 1", isPublic: true);
        await TestSeeder.SeedBoard(testScope, name: "Board 2", isPublic: false);

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
        await TestSeeder.SeedUser(testScope);
        var createdBoardId = await TestSeeder.SeedBoard(testScope, name: "Get By Id Test", isPublic: true);

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
        await TestSeeder.SeedUser(testScope);
        var createdBoardId = await TestSeeder.SeedBoard(testScope, name:"Before Update", isPublic: true);
        var updatePayload = new UpdateBoardDTO { Name = "After Update", IsPublic = false };

        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.PutAsJsonAsync($"{BASE_URL}{createdBoardId}", updatePayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedBoard = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(createdBoardId);
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
        await TestSeeder.SeedUser(testScope);
        var createdBoardId = await TestSeeder.SeedBoard(testScope, name:"To Be Deleted", isPublic:true);

        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.DeleteAsync($"{BASE_URL}{createdBoardId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedBoard = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(createdBoardId);
        Assert.Null(deletedBoard);
    }
}