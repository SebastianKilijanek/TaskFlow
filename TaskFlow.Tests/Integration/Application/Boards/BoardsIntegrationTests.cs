using Xunit;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Handlers;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests.Integration.Application.Boards;

[Collection("SequentialTests")]
public class BoardsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateBoardCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var handler = new CreateBoardHandler(testScope.UnitOfWork);
        var command = new CreateBoardCommand("Integration", true);

        // Act
        var boardId = await handler.Handle(command, default);

        // Assert
        var board = await testScope.DbContext.Boards.FindAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("Integration", board.Name);
        Assert.True(board.IsPublic);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsAllBoards()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        testScope.DbContext.Boards.Add(new Board { Id = Guid.NewGuid(), Name = "Board1", IsPublic = true });
        testScope.DbContext.Boards.Add(new Board { Id = Guid.NewGuid(), Name = "Board2", IsPublic = false });
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetBoardsHandler(testScope.UnitOfWork, testScope.Mapper);

        // Act
        var boards = await handler.Handle(new GetBoardsQuery(), default);

        // Assert
        Assert.NotNull(boards);
        Assert.Equal(2, boards.Count());
        Assert.Contains(boards, b => b.Name == "Board1" && b.IsPublic);
        Assert.Contains(boards, b => b.Name == "Board2" && !b.IsPublic);
        Assert.DoesNotContain(boards, b => b.Name == "NonExistentBoard");
        Assert.DoesNotContain(boards, b => b.Name == "Board3" && b.IsPublic);
    }

    [Fact]
    public async Task UpdateBoardCommand_UpdatesExistingBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "OldName", IsPublic = false };
        testScope.DbContext.Boards.Add(board);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new UpdateBoardHandler(testScope.UnitOfWork);
        var command = new UpdateBoardCommand(board.Id, "NewName", true);

        // Act
        await handler.Handle(command, default);

        // Assert
        var updatedBoard = await testScope.DbContext.Boards.FindAsync(board.Id);
        Assert.NotNull(updatedBoard);
        Assert.Equal("NewName", updatedBoard.Name);
        Assert.True(updatedBoard.IsPublic);
    }

    [Fact]
    public async Task DeleteBoardCommand_RemovesBoardFromDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "ToDelete", IsPublic = true };
        testScope.DbContext.Boards.Add(board);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new DeleteBoardHandler(testScope.UnitOfWork);

        // Act
        await handler.Handle(new DeleteBoardCommand(board.Id), default);
        
        // Assert
        var deletedBoard = await testScope.DbContext.Boards.FindAsync(board.Id);
        Assert.Null(deletedBoard);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ReturnsCorrectBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "TestBoard", IsPublic = true };
        testScope.DbContext.Boards.Add(board);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetBoardByIdHandler(testScope.UnitOfWork, testScope.Mapper);

        // Act
        var result = await handler.Handle(new GetBoardByIdQuery(board.Id), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(board.Id, result.Id);
        Assert.Equal("TestBoard", result.Name);
        Assert.True(result.IsPublic);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ReturnsNullForNonExistentBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var handler = new GetBoardByIdHandler(testScope.UnitOfWork, testScope.Mapper);

        // Act
        var result = await handler.Handle(new GetBoardByIdQuery(Guid.NewGuid()), default);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsEmptyListWhenNoBoardsExist()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var handler = new GetBoardsHandler(testScope.UnitOfWork, testScope.Mapper);

        // Act
        var result = await handler.Handle(new GetBoardsQuery(), default);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}