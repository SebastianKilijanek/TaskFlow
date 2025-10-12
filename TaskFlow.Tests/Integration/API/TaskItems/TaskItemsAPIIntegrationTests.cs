using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Integration.API.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string BOARDS_BASE_URL = "/api/v1/boards";
    private const string COLUMNS_BASE_URL = "/api/columns";
    private const string TASKS_BASE_URL = "/api/taskitems";
    private readonly Guid _userId = Guid.Parse(TestAuthHandler.UserId);

    private async Task SeedUser(TestScope scope)
    {
        var user = new User { Id = _userId, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash"};
        await scope.DbContext.Users.AddAsync(user);
        await scope.DbContext.SaveChangesAsync();
    }
    
    private async Task<Guid> CreateTestBoard(HttpClient client, string name = "Test Board")
    {
        var payload = new CreateBoardDTO { Name = name, IsPublic = false };
        var response = await client.PostAsJsonAsync(BOARDS_BASE_URL, payload);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        return Guid.Parse(location!.Segments.Last());
    }

    private async Task<Guid> CreateTestColumn(HttpClient client, Guid boardId, string name = "Test Column")
    {
        var payload = new CreateColumnDTO { Name = name, BoardId = boardId };
        var response = await client.PostAsJsonAsync(COLUMNS_BASE_URL, payload);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        return Guid.Parse(location!.Segments.Last());
    }

    private async Task<Guid> CreateTestTask(HttpClient client, Guid columnId, string title = "Test Task")
    {
        var payload = new CreateTaskItemDTO { Title = title, Description = "Test Description", ColumnId = columnId };
        var response = await client.PostAsJsonAsync(TASKS_BASE_URL, payload);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        return Guid.Parse(location!.Segments.Last());
    }

    [Fact]
    public async Task Post_CreateTaskItem_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);
        var payload = new CreateTaskItemDTO { Title = "New API Task", Description = "From API test", ColumnId = columnId };

        // Act
        var response = await client.PostAsJsonAsync(TASKS_BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var taskId = Guid.Parse(location.Segments.Last());
        var task = await testScope.DbContext.TaskItems.FindAsync(taskId);
        Assert.NotNull(task);
        Assert.Equal("New API Task", task.Title);
        Assert.Equal(0, task.Position);
    }

    [Fact]
    public async Task Get_TaskItemById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);
        var taskId = await CreateTestTask(client, columnId);

        // Act
        var response = await client.GetAsync($"{TASKS_BASE_URL}/{taskId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskItemDTO>();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
    }

    [Fact]
    public async Task Get_TaskItemsByColumn_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);
        await CreateTestTask(client, columnId, "Task 1");
        await CreateTestTask(client, columnId, "Task 2");

        // Act
        var response = await client.GetAsync($"{TASKS_BASE_URL}/column/{columnId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItemDTO>>();
        Assert.NotNull(tasks);
        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task Put_UpdateTaskItem_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);
        var taskId = await CreateTestTask(client, columnId);
        var payload = new UpdateTaskItemDTO { Title = "New Title", Description = "New Desc", Status = 1 };

        // Act
        var response = await client.PutAsJsonAsync($"{TASKS_BASE_URL}/{taskId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedTask = await testScope.DbContext.TaskItems.FindAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal("New Title", updatedTask.Title);
        Assert.Equal("New Desc", updatedTask.Description);
    }

    [Fact]
    public async Task Delete_TaskItem_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var columnId = await CreateTestColumn(client, boardId);
        var taskId = await CreateTestTask(client, columnId);

        // Act
        var response = await client.DeleteAsync($"{TASKS_BASE_URL}/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedTask = await testScope.DbContext.TaskItems.FindAsync(taskId);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task Post_MoveTaskItem_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await CreateTestBoard(client);
        var column1Id = await CreateTestColumn(client, boardId, "Column 1");
        var column2Id = await CreateTestColumn(client, boardId, "Column 2");
        var taskToMoveId = await CreateTestTask(client, column1Id);
        await CreateTestTask(client, column1Id, "Other Task");
        var payload = new MoveTaskItemDTO { NewColumnId = column2Id, NewPosition = 0 };

        // Act
        var response = await client.PostAsJsonAsync($"{TASKS_BASE_URL}/{taskToMoveId}/move", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var movedTask = await testScope.DbContext.TaskItems.FindAsync(taskToMoveId);
        Assert.NotNull(movedTask);
        Assert.Equal(column2Id, movedTask.ColumnId);
        Assert.Equal(0, movedTask.Position);
    }
}