using System.Linq.Expressions;
using AutoMapper;
using Moq;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Boards.Handlers;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.Boards;

public class BoardsHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepository<Board>> _boardRepositoryMock;
    private readonly Mock<IRepository<UserBoard>> _userBoardRepositoryMock;
    private readonly Guid _userId = Guid.NewGuid();

    public BoardsHandlersTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _boardRepositoryMock = new Mock<IRepository<Board>>();
        _userBoardRepositoryMock = new Mock<IRepository<UserBoard>>();

        _unitOfWorkMock.Setup(uow => uow.Repository<Board>()).Returns(_boardRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.Repository<UserBoard>()).Returns(_userBoardRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateBoardHandler_Should_CreateBoardAndUserBoard()
    {
        // Arrange
        var handler = new CreateBoardHandler(_unitOfWorkMock.Object);
        var command = new CreateBoardCommand(_userId, "Test Board", true);

        // Act
        var boardId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, boardId);
        _boardRepositoryMock.Verify(r => r.AddAsync(It.Is<Board>(b => b.Id == boardId && b.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        _userBoardRepositoryMock.Verify(r => r.AddAsync(It.Is<UserBoard>(ub => ub.BoardId == boardId && ub.UserId == command.UserId && ub.BoardRole == BoardRole.Owner), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBoardsHandler_Should_ReturnUserBoards()
    {
        // Arrange
        var boardId1 = Guid.NewGuid();
        var boardId2 = Guid.NewGuid();
        var userBoards = new List<UserBoard>
        {
            new() { UserId = _userId, BoardId = boardId1 },
            new() { UserId = _userId, BoardId = boardId2 }
        };
        var boards = new List<Board>
        {
            new() { Id = boardId1, Name = "Board 1" },
            new() { Id = boardId2, Name = "Board 2" }
        };
        var boardDTOs = new List<BoardDTO>
        {
            new() { Id = boardId1, Name = "Board 1" },
            new() { Id = boardId2, Name = "Board 2" }
        };

        _userBoardRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<UserBoard, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBoards);
        _boardRepositoryMock.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Board, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<BoardDTO>>(boards)).Returns(boardDTOs);

        var handler = new GetBoardsHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetBoardsQuery(_userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Name == "Board 1");
    }

    [Fact]
    public async Task GetBoardByIdHandler_Should_ReturnCorrectBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Test Board" };
        var boardDTO = new BoardDTO { Id = boardId, Name = "Test Board" };
        
        _mapperMock.Setup(m => m.Map<BoardDTO>(board)).Returns(boardDTO);

        var handler = new GetBoardByIdHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetBoardByIdQuery(_userId, boardId) { Board = board };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.Id);
        Assert.Equal("Test Board", result.Name);
    }

    [Fact]
    public async Task UpdateBoardHandler_Should_UpdateBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Old Name", IsPublic = true };
        var handler = new UpdateBoardHandler(_unitOfWorkMock.Object);
        var command = new UpdateBoardCommand(_userId, boardId, "New Name", false) { Board = board };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _boardRepositoryMock.Verify(r => r.Update(It.Is<Board>(b => b.Id == boardId && b.Name == "New Name" && !b.IsPublic)), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBoardHandler_Should_RemoveBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "To Delete" };
        var handler = new DeleteBoardHandler(_unitOfWorkMock.Object);
        var command = new DeleteBoardCommand(_userId, boardId) { Board = board };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _boardRepositoryMock.Verify(r => r.Remove(It.Is<Board>(b => b.Id == boardId)), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}