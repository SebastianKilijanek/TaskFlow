using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.API.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string TASKS_BASE_URL = "/api/taskitems";

    [Fact]
    public async Task Post_CreateTaskItem_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var payload = new CreateTaskItemDTO { Title = "New API Task", Description = "From API test", ColumnId = columnId };

        // Act
        var response = await client.PostAsJsonAsync(TASKS_BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var taskId = Guid.Parse(location.Segments.Last());
        var task = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
        Assert.NotNull(task);
        Assert.Equal("New API Task", task.Title);
        Assert.Equal(0, task.Position);
    }

    [Fact]
    public async Task Get_TaskItemById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskId = await TestSeeder.SeedTask(testScope, columnId);

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
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        await TestSeeder.SeedTask(testScope, columnId, "Task 1");
        await TestSeeder.SeedTask(testScope, columnId, "Task 2");

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
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskId = await TestSeeder.SeedTask(testScope, columnId);
        var payload = new UpdateTaskItemDTO { Title = "New Title", Description = "New Desc", Status = (int)TaskItemStatus.InProgress };

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PutAsJsonAsync($"{TASKS_BASE_URL}/{taskId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal("New Title", updatedTask.Title);
        Assert.Equal("New Desc", updatedTask.Description);
        Assert.Equal(TaskItemStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task Delete_TaskItem_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskId = await TestSeeder.SeedTask(testScope, columnId);

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.DeleteAsync($"{TASKS_BASE_URL}/{taskId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task Post_MoveTaskItem_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var column1Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 1", 0);
        var column2Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 2", 1);
        var taskToMoveId = await TestSeeder.SeedTask(testScope, column1Id, "Task To Move", 0);
        await TestSeeder.SeedTask(testScope, column1Id, "Other Task", 1);
        var payload = new MoveTaskItemDTO { NewColumnId = column2Id, NewPosition = 0 };

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PostAsJsonAsync($"{TASKS_BASE_URL}/{taskToMoveId}/move", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var movedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskToMoveId);
        Assert.NotNull(movedTask);
        Assert.Equal(column2Id, movedTask.ColumnId);
        Assert.Equal(0, movedTask.Position);
    }
}