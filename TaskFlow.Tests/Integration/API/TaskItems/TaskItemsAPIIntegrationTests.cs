using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Integration.API.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(Guid boardId, Guid columnId)> CreateBoardAndColumn()
    {
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var testScope = factory.GetTestScope();
        await testScope.UnitOfWork.Repository<Board>().AddAsync(board);
        await testScope.UnitOfWork.SaveChangesAsync();

        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id, Position = 0 };
        await testScope.UnitOfWork.Repository<Column>().AddAsync(column);
        await testScope.UnitOfWork.SaveChangesAsync();

        return (board.Id, column.Id);
    }

    [Fact]
    public async Task Post_CreateTaskItem_Returns201()
    {
        // Arrange
        var (_, columnId) = await CreateBoardAndColumn();
        var payload = new CreateTaskItemCommand("Test Task", "Description", columnId, 0);

        // Act
        var result = await _client.PostAsJsonAsync("/api/taskitems", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Headers.Location);
    }

    [Fact]
    public async Task Get_TaskItemById_Returns200()
    {
        // Arrange
        var (_, columnId) = await CreateBoardAndColumn();
        var createPayload = new CreateTaskItemCommand("Test Task", "Description", columnId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/taskitems", createPayload);
        var createdTaskItemUrl = createResponse.Headers.Location;

        // Act
        var result = await _client.GetAsync(createdTaskItemUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Contains("Test Task", content);
    }

    [Fact]
    public async Task Put_UpdateTaskItem_Returns204()
    {
        // Arrange
        var (_, columnId) = await CreateBoardAndColumn();
        var createPayload = new CreateTaskItemCommand("Test Task", "Description", columnId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/taskitems", createPayload);
        var createdTaskItemUrl = createResponse.Headers.Location!;
        var idSegment = createdTaskItemUrl.Segments.Last();
        var createdTaskItemId = Guid.Parse(idSegment);

        var updatePayload = new UpdateTaskItemCommand(createdTaskItemId, "Updated Task", "Updated Desc", 0, null);

        // Act
        var result = await _client.PutAsJsonAsync($"/api/taskitems/{createdTaskItemId}", updatePayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task Delete_TaskItem_Returns204()
    {
        // Arrange
        var (_, columnId) = await CreateBoardAndColumn();
        var createPayload = new CreateTaskItemCommand("Test Task", "Description", columnId, 0);
        var createResponse = await _client.PostAsJsonAsync("/api/taskitems", createPayload);
        var createdTaskItemUrl = createResponse.Headers.Location!;
        var idSegment = createdTaskItemUrl.Segments.Last();
        var createdTaskItemId = Guid.Parse(idSegment);

        // Act
        var result = await _client.DeleteAsync($"/api/taskitems/{createdTaskItemId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}