using Moq;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.Handlers;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using AutoMapper;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.Columns;

public class ColumnsHandlersTests
{
    [Fact]
    public async Task CreateColumn_ShouldReturnColumnId()
    {
        var repoMock = new Mock<IRepository<Column>>();
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Column>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateColumnHandler(uowMock.Object);
        var command = new CreateColumnCommand("TestColumn", Guid.NewGuid(), 1);

        var result = await handler.Handle(command, default);

        Assert.NotEqual(Guid.Empty, result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Column>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetColumnsByBoard_ShouldReturnColumnsList()
    {
        var boardId = Guid.NewGuid();
        var columns = new List<Column>
        {
            new Column { Id = Guid.NewGuid(), Name = "Col1", BoardId = boardId, Position = 1 },
            new Column { Id = Guid.NewGuid(), Name = "Col2", BoardId = boardId, Position = 2 }
        };

        var repoMock = new Mock<IRepository<Column>>();
        repoMock.Setup(r => r.ListAsync(It.IsAny<Predicate<Column>>())).ReturnsAsync(columns);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Column>()).Returns(repoMock.Object);

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<ColumnDTO>(It.IsAny<Column>()))
            .Returns<Column>(c => new ColumnDTO { Id = c.Id, Name = c.Name, BoardId = c.BoardId, Position = c.Position });

        var handler = new GetColumnsByBoardHandler(uowMock.Object, mapperMock.Object);

        var result = await handler.Handle(new GetColumnsByBoardQuery(boardId), default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetColumnById_ShouldReturnColumnDTO()
    {
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, Name = "Col", BoardId = Guid.NewGuid(), Position = 1 };

        var repoMock = new Mock<IRepository<Column>>();
        repoMock.Setup(r => r.GetByIdAsync(columnId)).ReturnsAsync(column);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Column>()).Returns(repoMock.Object);

        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<ColumnDTO>(column))
            .Returns(new ColumnDTO { Id = column.Id, Name = column.Name, BoardId = column.BoardId, Position = column.Position });

        var handler = new GetColumnByIdHandler(uowMock.Object, mapperMock.Object);

        var result = await handler.Handle(new GetColumnByIdQuery(columnId), default);

        Assert.NotNull(result);
        Assert.Equal(columnId, result!.Id);
    }

    [Fact]
    public async Task UpdateColumn_ShouldChangeProperties()
    {
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, Name = "Old", Position = 1, BoardId = Guid.Empty};

        var repoMock = new Mock<IRepository<Column>>();
        repoMock.Setup(r => r.GetByIdAsync(columnId)).ReturnsAsync(column);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Column>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateColumnHandler(uowMock.Object);
        var command = new UpdateColumnCommand(columnId, "New", 2);

        await handler.Handle(command, default);

        Assert.Equal("New", column.Name);
        Assert.Equal(2, column.Position);
        repoMock.Verify(r => r.Update(column), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteColumn_ShouldRemoveColumn()
    {
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, Name = "ToDelete", Position = 1, BoardId = Guid.Empty};

        var repoMock = new Mock<IRepository<Column>>();
        repoMock.Setup(r => r.GetByIdAsync(columnId)).ReturnsAsync(column);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Column>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteColumnHandler(uowMock.Object);
        var command = new DeleteColumnCommand(columnId);

        await handler.Handle(command, default);

        repoMock.Verify(r => r.Remove(column), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}