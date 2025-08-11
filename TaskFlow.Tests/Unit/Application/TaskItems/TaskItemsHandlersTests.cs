using AutoMapper;
using Moq;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Application.TaskItems.Handlers;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.TaskItems;

public class TaskItemsHandlersTests
{
    [Fact]
    public async Task CreateTaskItem_ShouldReturnTaskItemId()
    {
        var repoMock = new Mock<IRepository<TaskItem>>();
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateTaskItemHandler(uowMock.Object);
        var command = new CreateTaskItemCommand("Title", "Desc", Guid.NewGuid(), 1);

        var result = await handler.Handle(command, default);

        Assert.NotEqual(Guid.Empty, result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTaskItemById_ShouldReturnTaskItemDTO()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskId, Title = "T", ColumnId = Guid.NewGuid(), Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<TaskItemDTO>(taskItem)).Returns(new TaskItemDTO { Id = taskId, Title = "T", ColumnId = taskItem.ColumnId, Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = taskItem.CreatedAt });

        var handler = new GetTaskItemByIdHandler(uowMock.Object, mapperMock.Object);

        var result = await handler.Handle(new GetTaskItemByIdQuery(taskId), default);

        Assert.NotNull(result);
        Assert.Equal(taskId, result!.Id);
    }

    [Fact]
    public async Task GetTaskItemsByColumn_ShouldReturnTaskItemDTOs()
    {
        var columnId = Guid.NewGuid();
        var taskItems = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "A", ColumnId = columnId, Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Title = "B", ColumnId = columnId, Position = 2, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow }
        };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.ListAsync(It.IsAny<Predicate<TaskItem>>())).ReturnsAsync(taskItems);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<TaskItemDTO>(It.IsAny<TaskItem>()))
            .Returns<TaskItem>(t => new TaskItemDTO { Id = t.Id, Title = t.Title, ColumnId = t.ColumnId, Position = t.Position, Status = t.Status, CreatedAt = t.CreatedAt });

        var handler = new GetTaskItemsByColumnHandler(uowMock.Object, mapperMock.Object);

        var result = await handler.Handle(new GetTaskItemsByColumnQuery(columnId), default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateTaskItem_ShouldChangeProperties()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskId, Title = "Old", Description = "OldDesc", Position = 1, ColumnId = Guid.NewGuid(), Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateTaskItemHandler(uowMock.Object);
        var command = new UpdateTaskItemCommand(taskId, "New", "NewDesc", 2, null);

        await handler.Handle(command, default);

        Assert.Equal("New", taskItem.Title);
        Assert.Equal("NewDesc", taskItem.Description);
        Assert.Equal(2, taskItem.Position);
        repoMock.Verify(r => r.Update(taskItem), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MoveTaskItem_ShouldChangeColumnAndPosition()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskId, Title = "T", ColumnId = Guid.NewGuid(), Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new MoveTaskItemHandler(uowMock.Object);
        var newColumnId = Guid.NewGuid();
        var command = new MoveTaskItemCommand(taskId, newColumnId, 5);

        await handler.Handle(command, default);

        Assert.Equal(newColumnId, taskItem.ColumnId);
        Assert.Equal(5, taskItem.Position);
        repoMock.Verify(r => r.Update(taskItem), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangeTaskItemStatus_ShouldUpdateStatus()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskId, Title = "T", ColumnId = Guid.NewGuid(), Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new ChangeTaskItemStatusHandler(uowMock.Object);
        var command = new ChangeTaskItemStatusCommand(taskId, TaskItemStatus.Done.ToString());

        await handler.Handle(command, default);

        Assert.Equal(TaskItemStatus.Done, taskItem.Status);
        repoMock.Verify(r => r.Update(taskItem), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskItem_ShouldRemoveTaskItem()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem { Id = taskId, Title = "T", ColumnId = Guid.NewGuid(), Position = 1, Status = TaskItemStatus.ToDo, CreatedAt = DateTime.UtcNow };

        var repoMock = new Mock<IRepository<TaskItem>>();
        repoMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<TaskItem>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteTaskItemHandler(uowMock.Object);
        var command = new DeleteTaskItemCommand(taskId);

        await handler.Handle(command, default);

        repoMock.Verify(r => r.Remove(taskItem), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}