using Xunit;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.Handlers;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Integration.Application.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateTaskItemCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new CreateTaskItemHandler(testScope.UnitOfWork);
        var command = new CreateTaskItemCommand("New Task", "Description", column.Id, 1);

        // Act
        var taskItemId = await handler.Handle(command, default);

        // Assert
        var taskItem = await testScope.DbContext.TaskItems.FindAsync(taskItemId);
        Assert.NotNull(taskItem);
        Assert.Equal("New Task", taskItem.Title);
        Assert.Equal(column.Id, taskItem.ColumnId);
    }

    [Fact]
    public async Task UpdateTaskItemCommand_UpdatesExistingTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Old Title", ColumnId = column.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.TaskItems.AddAsync(taskItem);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new UpdateTaskItemHandler(testScope.UnitOfWork);
        var command = new UpdateTaskItemCommand(taskItem.Id, "New Title", "New Desc", 2, null);

        // Act
        await handler.Handle(command, default);

        // Assert
        var updatedTaskItem = await testScope.DbContext.TaskItems.FindAsync(taskItem.Id);
        Assert.NotNull(updatedTaskItem);
        Assert.Equal("New Title", updatedTaskItem.Title);
        Assert.Equal("New Desc", updatedTaskItem.Description);
        Assert.Equal(2, updatedTaskItem.Position);
    }

    [Fact]
    public async Task DeleteTaskItemCommand_RemovesTaskItemFromDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "To Delete", ColumnId = column.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.TaskItems.AddAsync(taskItem);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new DeleteTaskItemHandler(testScope.UnitOfWork);
        var command = new DeleteTaskItemCommand(taskItem.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var deletedTaskItem = await testScope.DbContext.TaskItems.FindAsync(taskItem.Id);
        Assert.Null(deletedTaskItem);
    }

    [Fact]
    public async Task GetTaskItemByIdQuery_ReturnsCorrectTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Test Task", ColumnId = column.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.TaskItems.AddAsync(taskItem);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetTaskItemByIdHandler(testScope.UnitOfWork, testScope.Mapper);
        var query = new GetTaskItemByIdQuery(taskItem.Id);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskItem.Id, result.Id);
        Assert.Equal(taskItem.Title, result.Title);
    }

    [Fact]
    public async Task GetTaskItemsByColumnQuery_ReturnsTaskItemsForColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = board.Id };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", BoardId = board.Id };
        var taskItem1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", ColumnId = column1.Id, Position = 1, Status = TaskItemStatus.ToDo };
        var taskItem2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", ColumnId = column1.Id, Position = 2, Status = TaskItemStatus.ToDo };
        var taskItem3 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 3", ColumnId = column2.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.AddRangeAsync(board, column1, column2, taskItem1, taskItem2, taskItem3);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetTaskItemsByColumnHandler(testScope.UnitOfWork, testScope.Mapper);
        var query = new GetTaskItemsByColumnQuery(column1.Id);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == taskItem1.Id);
        Assert.Contains(result, t => t.Id == taskItem2.Id);
        Assert.DoesNotContain(result, t => t.Id == taskItem3.Id);
    }

    [Fact]
    public async Task MoveTaskItemCommand_UpdatesColumnAndPosition()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = board.Id };
        var column2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", BoardId = board.Id };
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Task to Move", ColumnId = column1.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.AddRangeAsync(board, column1, column2, taskItem);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new MoveTaskItemHandler(testScope.UnitOfWork);
        var command = new MoveTaskItemCommand(taskItem.Id, column2.Id, 5);

        // Act
        await handler.Handle(command, default);

        // Assert
        var movedTaskItem = await testScope.DbContext.TaskItems.FindAsync(taskItem.Id);
        Assert.NotNull(movedTaskItem);
        Assert.Equal(column2.Id, movedTaskItem.ColumnId);
        Assert.Equal(5, movedTaskItem.Position);
    }

    [Fact]
    public async Task ChangeTaskItemStatusCommand_UpdatesStatus()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Task", ColumnId = column.Id, Position = 1, Status = TaskItemStatus.ToDo };
        await testScope.DbContext.AddRangeAsync(board, column, taskItem);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new ChangeTaskItemStatusHandler(testScope.UnitOfWork);
        var command = new ChangeTaskItemStatusCommand(taskItem.Id, TaskItemStatus.InProgress.ToString());

        // Act
        await handler.Handle(command, default);

        // Assert
        var updatedTaskItem = await testScope.DbContext.TaskItems.FindAsync(taskItem.Id);
        Assert.NotNull(updatedTaskItem);
        Assert.Equal(TaskItemStatus.InProgress, updatedTaskItem.Status);
    }
}