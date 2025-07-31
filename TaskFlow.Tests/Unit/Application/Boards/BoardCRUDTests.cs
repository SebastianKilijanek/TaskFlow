using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Handlers;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Domain.Interfaces;
using Xunit;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests.Application.Boards;

public class BoardCrudTests : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly IMapper _mapper;
        
    public BoardCrudTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    {
        using var scope = factory.Services.CreateScope();
        _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    }
        
    [Fact]
    public async Task CreateBoard_ShouldReturnBoardId()
    {
        var repoMock = new Mock<IRepository<Board>>();
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Board>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateBoardHandler(uowMock.Object);
        var command = new CreateBoardCommand("TestBoard", true);

        var result = await handler.Handle(command, default);

        Assert.NotEqual(Guid.Empty, result);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Board>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBoards_ShouldReturnBoardsList()
    {
        var boards = new List<Board>
        {
            new Board { Id = Guid.NewGuid(), Name = "Board1", IsPublic = true },
            new Board { Id = Guid.NewGuid(), Name = "Board2", IsPublic = false }
        };

        var repoMock = new Mock<IRepository<Board>>();
        repoMock.Setup(r => r.ListAsync()).ReturnsAsync(boards);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Board>()).Returns(repoMock.Object);

        var handler = new GetBoardsHandler(uowMock.Object, _mapper);

        var result = await handler.Handle(new GetBoardsQuery(), default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateBoard_ShouldChangeProperties()
    {
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "OldName", IsPublic = false };

        var repoMock = new Mock<IRepository<Board>>();
        repoMock.Setup(r => r.GetByIdAsync(boardId)).ReturnsAsync(board);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Board>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateBoardHandler(uowMock.Object);
        var command = new UpdateBoardCommand(boardId, "NewName", true);

        await handler.Handle(command, default);

        Assert.Equal("NewName", board.Name);
        Assert.True(board.IsPublic);
        repoMock.Verify(r => r.Update(board), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBoard_ShouldRemoveBoard()
    {
        var boardId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "BoardToDelete", IsPublic = true };

        var repoMock = new Mock<IRepository<Board>>();
        repoMock.Setup(r => r.GetByIdAsync(boardId)).ReturnsAsync(board);
        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Repository<Board>()).Returns(repoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteBoardHandler(uowMock.Object);
        var command = new DeleteBoardCommand(boardId);

        await handler.Handle(command, default);

        repoMock.Verify(r => r.Remove(board), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}