using System.Linq.Expressions;
using AutoMapper;
using Moq;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.Handlers;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.TaskItems;

public class TaskItemsHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<TaskItem>> _taskItemRepositoryMock;
    private readonly Mock<IRepository<Column>> _columnRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskItemsHandlersTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskItemRepositoryMock = new Mock<IRepository<TaskItem>>();
        _columnRepositoryMock = new Mock<IRepository<Column>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Repository<Column>()).Returns(_columnRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<TaskItem>()).Returns(_taskItemRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateTaskItemHandler_Should_AddTaskItem()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var command = new CreateTaskItemCommand(_userId, "New Task", "Description", columnId);
        var handler = new CreateTaskItemHandler(_unitOfWorkMock.Object);

        _taskItemRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _taskItemRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t =>
            t.Title == "New Task" &&
            t.ColumnId == columnId &&
            t.Position == 0
        ), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task DeleteTaskItemHandler_Should_RemoveTaskItemAndReorder()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var taskToDelete = new TaskItem { Id = Guid.NewGuid(), Title = "To Delete", ColumnId = columnId, Position = 0 };
        var otherTask = new TaskItem { Id = Guid.NewGuid(), Title = "Other Task", ColumnId = columnId, Position = 1 };
        var tasks = new List<TaskItem> { taskToDelete, otherTask };

        _taskItemRepositoryMock.Setup(r => r.ListAsync(t => t.ColumnId == columnId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new DeleteTaskItemHandler(_unitOfWorkMock.Object);
        var command = new DeleteTaskItemCommand(_userId, taskToDelete.Id) { Entity = taskToDelete };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _taskItemRepositoryMock.Verify(r => r.Remove(taskToDelete), Times.Once);
        _taskItemRepositoryMock.Verify(r => r.Update(It.Is<TaskItem>(t => t.Id == otherTask.Id && t.Position == 0)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskItemHandler_Should_ChangeTaskItemProperties()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            Description = "Old Desc",
            Status = TaskItemStatus.ToDo,
            ColumnId = columnId,
            Column = new Column { Id = columnId, BoardId = Guid.NewGuid(), Name = "Test Column" }
        };
        var handler = new UpdateTaskItemHandler(_unitOfWorkMock.Object);
        var command = new UpdateTaskItemCommand(_userId, taskItem.Id, "New Title", "New Desc", 
            (int)TaskItemStatus.InProgress, null) { Entity = taskItem };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Title", taskItem.Title);
        Assert.Equal("New Desc", taskItem.Description);
        Assert.Equal(TaskItemStatus.InProgress, taskItem.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveTaskItemHandler_Should_ReorderTasks()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var Column = new Column { Id = columnId, Name = "Test Column", BoardId = boardId };
        var taskToMove = new TaskItem { Id = Guid.NewGuid(), Title = "Task to Move", ColumnId = columnId, Position = 1, Column = Column };
        var otherTask = new TaskItem { Id = Guid.NewGuid(), Title = "Other Task", ColumnId = columnId, Position = 0, Column = Column};
        var tasks = new List<TaskItem> { otherTask, taskToMove };

        _columnRepositoryMock.Setup(r => r.GetByIdAsync(columnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Column);
        _taskItemRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new MoveTaskItemHandler(_unitOfWorkMock.Object);
        var command = new MoveTaskItemCommand(_userId, taskToMove.Id, columnId, 0) { Entity = taskToMove };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, taskToMove.Position);
        Assert.Equal(1, otherTask.Position);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveTaskItemHandler_Should_ChangeTaskColumn()
    {
        // Arrange
        var oldColumnId = Guid.NewGuid();
        var newColumnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var oldColumn = new Column { Id = oldColumnId, Name = "Old Column", BoardId = boardId };
        var newColumn = new Column { Id = newColumnId, Name = "New Column", BoardId = boardId };
        var taskToMove = new TaskItem { Id = Guid.NewGuid(), Title = "Task to Move", ColumnId = oldColumnId, Position = 0, Column = oldColumn };
        var otherTaskInOldColumn = new TaskItem { Id = Guid.NewGuid(), Title = "Other Task", ColumnId = oldColumnId, Position = 1, Column = oldColumn };
        var taskInNewColumn = new TaskItem { Id = Guid.NewGuid(), Title = "Task in New Column", ColumnId = newColumnId, Position = 0, Column = newColumn };

        var allTasks = new List<TaskItem> { taskToMove, otherTaskInOldColumn, taskInNewColumn };

        _columnRepositoryMock.Setup(r => r.GetByIdAsync(newColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newColumn);

        _taskItemRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<TaskItem, bool>> predicate, CancellationToken token) =>
            {
                var compiledPredicate = predicate.Compile();
                return allTasks.Where(compiledPredicate).ToList();
            });

        var handler = new MoveTaskItemHandler(_unitOfWorkMock.Object);
        var command = new MoveTaskItemCommand(_userId, taskToMove.Id, newColumnId, 1) { Entity = taskToMove };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newColumnId, taskToMove.ColumnId);
        Assert.Equal(1, taskToMove.Position);
        Assert.Equal(0, otherTaskInOldColumn.Position);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskItemByIdHandler_Should_ReturnCorrectTaskItem()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskItemId, Title = "Test Task", ColumnId = Guid.NewGuid() };
        var taskItemDTO = new TaskItemDTO { Id = taskItemId, Title = "Test Task", ColumnId = taskItem.ColumnId };

        _mapperMock.Setup(m => m.Map<TaskItemDTO>(taskItem)).Returns(taskItemDTO);
        var handler = new GetTaskItemByIdHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetTaskItemByIdQuery(_userId, taskItemId) { Entity = taskItem };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskItemId, result.Id);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetTaskItemsByColumnHandler_Should_ReturnTasksForColumn()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var taskItems = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", ColumnId = columnId, Position = 0 },
            new() { Id = Guid.NewGuid(), Title = "Task 2", ColumnId = columnId, Position = 1 }
        };
        var taskItemDTOs = new List<TaskItemDTO>
        {
            new() { Id = taskItems[0].Id, Title = "Task 1", ColumnId = columnId, Position = 0 },
            new() { Id = taskItems[1].Id, Title = "Task 2", ColumnId = columnId, Position = 1 }
        };

        _taskItemRepositoryMock.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Expression<Func<TaskItem, bool>> predicate, CancellationToken token) =>
                        {
                            var compiledPredicate = predicate.Compile();
                            return taskItems.Where(compiledPredicate).ToList();
                        });
        _mapperMock.Setup(m => m.Map<TaskItemDTO>(taskItems[0])).Returns(taskItemDTOs[0]);
        _mapperMock.Setup(m => m.Map<TaskItemDTO>(taskItems[1])).Returns(taskItemDTOs[1]);

        var handler = new GetTaskItemsByColumnHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetTaskItemsByColumnQuery(_userId, columnId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Task 1");
        Assert.Equal(taskItemDTOs[0], result.First(r => r.Id == taskItemDTOs[0].Id));
        Assert.Equal(taskItemDTOs[1], result.First(r => r.Id == taskItemDTOs[1].Id));
    }
}