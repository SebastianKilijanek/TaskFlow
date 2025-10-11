using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Columns;

[Collection("SequentialTests")]
public class ColumnsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string BOARDS_BASE_URL = "/api/v1/boards";
    private const string COLUMNS_BASE_URL = "/api/columns";
    private readonly Guid _userId = Guid.Parse(TestAuthHandler.UserId);

    private async Task SeedUser(TestScope scope)
    {
        var user = new User { Id = _userId, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash"};
        await scope.DbContext.Users.AddAsync(user);
        await scope.DbContext.SaveChangesAsync();
    }

    private async Task<Guid> CreateTestBoard(HttpClient client, string name = "Test Board", bool isPublic = true)
    {
        var payload = new CreateBoardDTO { Name = name, IsPublic = isPublic };
        var response = await client.PostAsJsonAsync(BOARDS_BASE_URL, payload);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        Assert.NotNull(location);
        return Guid.Parse(location.Segments.Last());
    }

    private async Task<Guid> CreateTestColumn(HttpClient client, Guid boardId, string name = "Test Column")
    {
        var payload = new CreateColumnDTO { Name = name, BoardId = boardId };
        var response = await client.PostAsJsonAsync(COLUMNS_BASE_URL, payload);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        Assert.NotNull(location);
        return Guid.Parse(location.Segments.Last());
    }

    [Fact]
    public async Task Post_CreateColumn_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var payload = new CreateColumnDTO { Name = "Test Column", BoardId = boardId };

        // Act
        var response = await client.PostAsJsonAsync(COLUMNS_BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var columnId = Guid.Parse(location.Segments.Last());
        var column = await testScope.DbContext.Columns.FindAsync(columnId);
        Assert.NotNull(column);
        Assert.Equal("Test Column", column.Name);
        Assert.Equal(boardId, column.BoardId);
    }

    [Fact]
    public async Task Get_ColumnsByBoard_Returns200WithColumns()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        await CreateTestColumn(client, boardId, "Column 1");
        await CreateTestColumn(client, boardId, "Column 2");

        // Act
        var response = await client.GetAsync($"{COLUMNS_BASE_URL}/board/{boardId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var columns = await response.Content.ReadFromJsonAsync<List<ColumnDTO>>();
        Assert.NotNull(columns);
        Assert.Equal(2, columns.Count);
    }

    [Fact]
    public async Task Get_ColumnById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId, "Test Column");

        // Act
        var response = await client.GetAsync($"{COLUMNS_BASE_URL}/{columnId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var column = await response.Content.ReadFromJsonAsync<ColumnDTO>();
        Assert.NotNull(column);
        Assert.Equal("Test Column", column.Name);
    }

    [Fact]
    public async Task Put_UpdateColumn_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId, "Old Name");
        var payload = new UpdateColumnDTO { Name = "New Name" };

        // Act
        var response = await client.PutAsJsonAsync($"{COLUMNS_BASE_URL}/{columnId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedColumn = await testScope.DbContext.Columns.FindAsync(columnId);
        Assert.NotNull(updatedColumn);
        Assert.Equal("New Name", updatedColumn.Name);
    }

    [Fact]
    public async Task Delete_Column_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);

        // Act
        var response = await client.DeleteAsync($"{COLUMNS_BASE_URL}/{columnId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedColumn = await testScope.DbContext.Columns.FindAsync(columnId);
        Assert.Null(deletedColumn);
    }

    [Fact]
    public async Task Patch_MoveColumn_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId1 = await CreateTestColumn(client, boardId, "Column 1");
        var columnId2 = await CreateTestColumn(client, boardId, "Column 2");
        var payload = new  MoveColumnDTO { NewPosition = 0 };

        // Act
        var response = await client.PatchAsJsonAsync($"{COLUMNS_BASE_URL}/{columnId2}/move", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var movedColumn = await testScope.DbContext.Columns.FindAsync(columnId2);
        Assert.NotNull(movedColumn);
        Assert.Equal(0, movedColumn.Position);
        var otherColumn = await testScope.DbContext.Columns.FindAsync(columnId1);
        Assert.NotNull(otherColumn);
        Assert.Equal(1, otherColumn.Position);
    }
}