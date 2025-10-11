using System.Linq.Expressions;
using AutoMapper;
using Moq;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.Columns.Handlers;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.Columns;

public class ColumnsHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepository<Column>> _columnRepositoryMock;
    private readonly Guid _userId = Guid.NewGuid();

    public ColumnsHandlersTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _columnRepositoryMock = new Mock<IRepository<Column>>();

        _unitOfWorkMock.Setup(uow => uow.Repository<Column>()).Returns(_columnRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateColumnHandler_Should_CreateColumnWithCorrectPosition()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var existingColumns = new List<Column>
            { new() { Id = Guid.NewGuid(), Name = "Test Column", BoardId = boardId, Position = 0 } };
        _columnRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Column, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingColumns);
        var handler = new CreateColumnHandler(_unitOfWorkMock.Object);
        var command = new CreateColumnCommand(_userId, "Test Column", boardId);

        // Act
        var columnId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, columnId);
        _columnRepositoryMock.Verify(r => r.AddAsync(It.Is<Column>(c => 
            c.Id == columnId && 
            c.Name == command.Name && 
            c.BoardId == command.BoardId && 
            c.Position == 1), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteColumnHandler_Should_RemoveColumnAndReorderOthers()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var columnToDelete = new Column { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 1", Position = 1 };
        var remainingColumns = new List<Column>
        {
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 2", Position = 0 },
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 3", Position = 2 }
        };
        _columnRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Column, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(remainingColumns);
        var handler = new DeleteColumnHandler(_unitOfWorkMock.Object);
        var command = new DeleteColumnCommand(_userId, columnToDelete.Id) { Entity = columnToDelete };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _columnRepositoryMock.Verify(r => r.Remove(It.Is<Column>(c => c.Id == columnToDelete.Id)), Times.Once);
        _columnRepositoryMock.Verify(r => r.Update(It.Is<Column>(c => c.Id == remainingColumns[1].Id && c.Position == 1)), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetColumnByIdHandler_Should_ReturnCorrectColumn()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, BoardId = boardId, Name = "Test Column" };
        var columnDTO = new ColumnDTO { Id = columnId, BoardId = boardId, Name = "Test Column" };
        _mapperMock.Setup(m => m.Map<ColumnDTO>(column)).Returns(columnDTO);
        var handler = new GetColumnByIdHandler(_mapperMock.Object);
        var query = new GetColumnByIdQuery(_userId, columnId) { Entity = column };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(columnId, result.Id);
        Assert.Equal("Test Column", result.Name);
    }

    [Fact]
    public async Task GetColumnsByBoardHandler_Should_ReturnColumnsForBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var columns = new List<Column>
        {
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 1", Position = 0 },
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 2", Position = 1 }
        };
        var columnDTOs = new List<ColumnDTO>
        {
            new() { Id = columns[0].Id, Name = "Column 1", Position = 0, BoardId = boardId },
            new() { Id = columns[1].Id, Name = "Column 2", Position = 1, BoardId = boardId }
        };

        _columnRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Column, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(columns);
    
        _mapperMock.Setup(m => m.Map<ColumnDTO>(columns[0])).Returns(columnDTOs[0]);
        _mapperMock.Setup(m => m.Map<ColumnDTO>(columns[1])).Returns(columnDTOs[1]);

        var handler = new GetColumnsByBoardHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetColumnsByBoardQuery(_userId, boardId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Column 1");
        Assert.Contains(result, c => c.Name == "Column 2");
    }

    [Fact]
    public async Task MoveColumnHandler_Should_UpdateColumnPosition()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var columnToMove = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = boardId, Position = 0 };
        var otherColumns = new List<Column>
        {
            columnToMove,
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 2", Position = 1 },
            new() { Id = Guid.NewGuid(), BoardId = boardId, Name = "Column 3", Position = 2 }
        };
        _columnRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Column, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherColumns);
        var handler = new MoveColumnHandler(_unitOfWorkMock.Object);
        var command = new MoveColumnCommand(_userId, columnToMove.Id, 2) { Entity = columnToMove };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _columnRepositoryMock.Verify(r => r.Update(It.Is<Column>(c => c.Id == columnToMove.Id && c.Position == 2)), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateColumnHandler_Should_UpdateColumnName()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, Name = "Old Name", BoardId = boardId};
        var handler = new UpdateColumnHandler(_unitOfWorkMock.Object);
        var command = new UpdateColumnCommand(_userId, columnId, "New Name") { Entity = column };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _columnRepositoryMock.Verify(r => r.Update(It.Is<Column>(c => c.Id == columnId && c.Name == "New Name")), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}