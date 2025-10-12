using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Integration.Application.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private async Task<(Guid userId, Guid boardId, Guid columnId, User user)> SeedUserAndBoard(TestScope testScope)
    {
        var user = new User
        {
            Id = Guid.Parse(TestAuthHandler.UserId), UserName = "Tester", Email = "test@test.com", PasswordHash = "hash"
        };
        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = true };
        var userBoard = new UserBoard { UserId = user.Id, BoardId = board.Id, BoardRole = BoardRole.Owner };
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = board.Id };

        await testScope.UnitOfWork.Repository<User>().AddAsync(user);
        await testScope.UnitOfWork.Repository<Board>().AddAsync(board);
        await testScope.UnitOfWork.Repository<UserBoard>().AddAsync(userBoard);
        await testScope.UnitOfWork.Repository<Column>().AddAsync(column);
        await testScope.UnitOfWork.SaveChangesAsync();

        return (user.Id, board.Id, column.Id, user);
    }

    [Fact]
    public async Task CreateTaskItemCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, _) = await SeedUserAndBoard(testScope);
        var command = new CreateTaskItemCommand(userId, "Integration Test Task", "Description", columnId);

        // Act
        var taskItemId = await mediator.Send(command);

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
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, _) = await SeedUserAndBoard(testScope);
        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", ColumnId = columnId, Position = 0 };
        var taskToDelete = new TaskItem { Id = Guid.NewGuid(), Title = "To Delete", ColumnId = columnId, Position = 1 };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", ColumnId = columnId, Position = 2 };
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(task1);
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(taskToDelete);
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(task2);
        await testScope.UnitOfWork.SaveChangesAsync();
    
        var taskIdToDelete = taskToDelete.Id;
        var task2Id = task2.Id;
    
        // Clear the context to avoid tracking issues
        testScope.DbContext.ChangeTracker.Clear();
    
        // Reload the entity to delete
        var trackedTaskToDelete = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskIdToDelete);
    
        var command = new DeleteTaskItemCommand(userId, taskIdToDelete) { Entity = trackedTaskToDelete! };

        // Act
        await mediator.Send(command);

        // Assert
        var deletedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskIdToDelete);
        Assert.Null(deletedTask);
        var reorderedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(task2Id);
        Assert.NotNull(reorderedTask);
        Assert.Equal(1, reorderedTask.Position);
    }

    [Fact]
    public async Task UpdateTaskItemCommand_UpdatesExistingTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, _) = await SeedUserAndBoard(testScope);
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Old Title", ColumnId = columnId, Position = 0 };
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(taskItem);
        await testScope.UnitOfWork.SaveChangesAsync();
        testScope.DbContext.Entry(taskItem).State = EntityState.Detached;

        var command = new UpdateTaskItemCommand(userId, taskItem.Id, "New Title", "New Desc", 
            (int)TaskItemStatus.InProgress, null) { Entity = taskItem };

        // Act
        await mediator.Send(command);

        // Assert
        var updatedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskItem.Id);
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
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, _) = await SeedUserAndBoard(testScope);
        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", ColumnId = columnId, Position = 0 };
        var taskToMove = new TaskItem { Id = Guid.NewGuid(), Title = "Task To Move", ColumnId = columnId, Position = 1 };
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(task1);
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(taskToMove);
        await testScope.UnitOfWork.SaveChangesAsync();

        var taskToMoveId = taskToMove.Id;
        var task1Id = task1.Id;

        // Clear the context to avoid tracking issues
        testScope.DbContext.ChangeTracker.Clear();

        // Reload the entity to move
        var trackedTaskToMove = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskToMoveId);

        var command = new MoveTaskItemCommand(userId, taskToMoveId, columnId, 0) { Entity = trackedTaskToMove! };

        // Act
        await mediator.Send(command);

        // Assert
        testScope.DbContext.ChangeTracker.Clear();
        var movedTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(taskToMoveId);
        var otherTask = await testScope.UnitOfWork.Repository<TaskItem>().GetByIdAsync(task1Id);
    
        Assert.NotNull(movedTask);
        Assert.NotNull(otherTask);
        Assert.Equal(0, movedTask.Position);
        Assert.Equal(1, otherTask.Position);
    }

    [Fact]
    public async Task GetTaskItemByIdQuery_ReturnsCorrectTaskItem()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, user) = await SeedUserAndBoard(testScope);
        var taskItem = new TaskItem { Id = Guid.NewGuid(), Title = "Get Me", ColumnId = columnId, Position = 0, AssignedUserId = userId, AssignedUser = user };
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(taskItem);
        await testScope.UnitOfWork.SaveChangesAsync();

        var query = new GetTaskItemByIdQuery(userId, taskItem.Id) { Entity = taskItem };

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskItem.Id, result.Id);
        Assert.Equal("Get Me", result.Title);
        Assert.Equal(user.UserName, result.AssignedUserName);
    }

    [Fact]
    public async Task GetTaskItemsByColumnQuery_ReturnsAllTasksInColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var mediator = testScope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var (userId, _, columnId, _) = await SeedUserAndBoard(testScope);
        var task1 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", ColumnId = columnId, Position = 0 };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", ColumnId = columnId, Position = 1 };
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(task1);
        await testScope.UnitOfWork.Repository<TaskItem>().AddAsync(task2);
        await testScope.UnitOfWork.SaveChangesAsync();

        var query = new GetTaskItemsByColumnQuery(userId, columnId);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Task 1");
        Assert.Contains(result, t => t.Title == "Task 2");
    }
}