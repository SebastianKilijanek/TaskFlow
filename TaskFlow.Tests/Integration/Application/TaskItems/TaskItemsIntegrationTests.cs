using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Application.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateTaskItemCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var command = new CreateTaskItemCommand(TestSeeder.DefaultUserId, "Integration Test Task", "Description", columnId);

        // Act
        var taskItemId = await testScope.Mediator.Send(command);

        // Assert
        var taskItem = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskItemId);
        Assert.NotNull(taskItem);
        Assert.Equal("Integration Test Task", taskItem.Title);
        Assert.Equal(columnId, taskItem.ColumnId);
        Assert.Equal(0, taskItem.Position);
    }

    [Fact]
    public async Task DeleteTaskItemCommand_RemovesFromDatabaseAndReorders()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var task1Id = await TestSeeder.SeedTask(testScope, columnId, "Task 1", 0);
        var taskToDeleteId = await TestSeeder.SeedTask(testScope, columnId, "To Delete", 1);
        var task2Id = await TestSeeder.SeedTask(testScope, columnId, "Task 2", 2);

        var command = new DeleteTaskItemCommand(TestSeeder.DefaultUserId, taskToDeleteId);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var deletedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskToDeleteId);
        Assert.Null(deletedTask);
        var remainingTask1 = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(task1Id);
        Assert.NotNull(remainingTask1);
        Assert.Equal(0, remainingTask1.Position);
        var reorderedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(task2Id);
        Assert.NotNull(reorderedTask);
        Assert.Equal(1, reorderedTask.Position);
    }

    [Fact]
    public async Task UpdateTaskItemCommand_UpdatesExistingTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId, "Old Title", 0);
        
        var command = new UpdateTaskItemCommand(TestSeeder.DefaultUserId, taskItemId, "New Title", "New Desc", 
            (int)TaskItemStatus.InProgress, null);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var updatedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskItemId);
        Assert.NotNull(updatedTask);
        Assert.Equal("New Title", updatedTask.Title);
        Assert.Equal("New Desc", updatedTask.Description);
        Assert.Equal(TaskItemStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task MoveTaskItemCommand_MovesTaskWithinColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var task1Id = await TestSeeder.SeedTask(testScope, columnId, "Task 1", 0);
        var taskToMoveId = await TestSeeder.SeedTask(testScope, columnId, "Task To Move", 1);

        var command = new MoveTaskItemCommand(TestSeeder.DefaultUserId, taskToMoveId, columnId, 0);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var movedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskToMoveId);
        var task1 = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(task1Id);
    
        Assert.NotNull(movedTask);
        Assert.NotNull(task1);
        Assert.Equal(0, movedTask.Position);
        Assert.Equal(1, task1.Position);
    }

    [Fact]
    public async Task GetTaskItemByIdQuery_ReturnsCorrectTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskId = await TestSeeder.SeedTask(testScope, columnId, "Get Me", 0);
        
        var query = new GetTaskItemByIdQuery(TestSeeder.DefaultUserId, taskId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Get Me", result.Title);
    }

    [Fact]
    public async Task GetTaskItemsByColumnQuery_ReturnsAllTasksInColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        await TestSeeder.SeedTask(testScope, columnId, "Task 1", 0);
        await TestSeeder.SeedTask(testScope, columnId, "Task 2", 1);
        
        var query = new GetTaskItemsByColumnQuery(TestSeeder.DefaultUserId, columnId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Task 1");
        Assert.Contains(result, t => t.Title == "Task 2");
    }
}