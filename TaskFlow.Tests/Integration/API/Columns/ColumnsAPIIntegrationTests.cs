using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.API.Columns;

[Collection("SequentialTests")]
public class ColumnsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string COLUMNS_BASE_URL = "/api/columns";
    
    [Fact]
    public async Task Post_CreateColumn_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var payload = new CreateColumnDTO { Name = "Test Column", BoardId = boardId };

        // Act
        var response = await client.PostAsJsonAsync(COLUMNS_BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var columnId = Guid.Parse(location.Segments.Last());
        var column = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId);
        Assert.NotNull(column);
        Assert.Equal("Test Column", column.Name);
        Assert.Equal(boardId, column.BoardId);
    }

    [Fact]
    public async Task Get_ColumnsByBoard_Returns200WithColumns()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        await TestSeeder.SeedColumn(testScope, boardId, "Column 1");
        await TestSeeder.SeedColumn(testScope, boardId, "Column 2");

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
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId, "Test Column");

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
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId, "Old Name");
        var payload = new UpdateColumnDTO { Name = "New Name" };

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PutAsJsonAsync($"{COLUMNS_BASE_URL}/{columnId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedColumn = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId);
        Assert.NotNull(updatedColumn);
        Assert.Equal("New Name", updatedColumn.Name);
    }

    [Fact]
    public async Task Delete_Column_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.DeleteAsync($"{COLUMNS_BASE_URL}/{columnId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedColumn = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId);
        Assert.Null(deletedColumn);
    }

    [Fact]
    public async Task Patch_MoveColumn_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId1 = await TestSeeder.SeedColumn(testScope, boardId, "Column 1", 0);
        var columnId2 = await TestSeeder.SeedColumn(testScope, boardId, "Column 2", 1);
        var payload = new MoveColumnDTO { NewPosition = 0 };

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PatchAsJsonAsync($"{COLUMNS_BASE_URL}/{columnId2}/move", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var movedColumn = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId2);
        Assert.NotNull(movedColumn);
        Assert.Equal(0, movedColumn.Position);
        var otherColumn = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId1);
        Assert.NotNull(otherColumn);
        Assert.Equal(1, otherColumn.Position);
    }
}