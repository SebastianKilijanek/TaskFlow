using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Application.Boards;

[Collection("SequentialTests")]
public class BoardsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateBoardCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var command = new CreateBoardCommand(TestSeeder.DefaultUserId, "Integration", true);

        // Act
        var boardId = await testScope.Mediator.Send(command);

        // Assert
        var board = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("Integration", board.Name);
        Assert.True(board.IsPublic);

        var userBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(TestSeeder.DefaultUserId, boardId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Owner, userBoard.BoardRole);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsOnlyUserMemberBoards()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        
        await TestSeeder.SeedUser(testScope);
        var otherUser = await TestSeeder.SeedUser(testScope, Guid.NewGuid(), "othertest@test.com", "OtherTester");
        var board1Id = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "Board1", true, BoardRole.Owner);
        var board2Id = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "Board2", false, BoardRole.Editor);
        await TestSeeder.SeedBoard(testScope, otherUser.Id, "Board3", false, BoardRole.Owner);

        var query = new GetBoardsQuery(TestSeeder.DefaultUserId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Id == board1Id);
        Assert.Contains(result, b => b.Id == board2Id);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsEmptyListWhenNoBoardsExist()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var query = new GetBoardsQuery(TestSeeder.DefaultUserId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateBoardCommand_UpdatesDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "Old Name", false, BoardRole.Owner);
        await testScope.UnitOfWork.SaveChangesAsync();

        var command = new UpdateBoardCommand(TestSeeder.DefaultUserId, boardId, "New Name", false);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var board = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("New Name", board.Name);
        Assert.False(board.IsPublic);
    }

    [Fact]
    public async Task DeleteBoardCommand_RemovesFromDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "To Delete", false, BoardRole.Owner);
        await testScope.UnitOfWork.SaveChangesAsync();

        var command = new DeleteBoardCommand(TestSeeder.DefaultUserId, boardId);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var board = await testScope.UnitOfWork.Repository<Board>().GetByIdAsync(boardId);
        Assert.Null(board);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ReturnsCorrectBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "Test Board", false, BoardRole.Viewer);
        await testScope.UnitOfWork.SaveChangesAsync();

        var query = new GetBoardByIdQuery(TestSeeder.DefaultUserId, boardId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.Id);
        Assert.Equal("Test Board", result.Name);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ThrowsNotFoundException_WhenBoardDoesNotExist()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var query = new GetBoardByIdQuery(TestSeeder.DefaultUserId, Guid.NewGuid());

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => testScope.Mediator.Send(query));
    }
    
    [Fact]
    public async Task UpdateBoardCommand_ThrowsForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, TestSeeder.DefaultUserId, "Old Name", false, BoardRole.Editor);
        await testScope.UnitOfWork.SaveChangesAsync();

        var command = new UpdateBoardCommand(TestSeeder.DefaultUserId, boardId, "New Name", true);

        // Act
        // Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => testScope.Mediator.Send(command));
    }
}