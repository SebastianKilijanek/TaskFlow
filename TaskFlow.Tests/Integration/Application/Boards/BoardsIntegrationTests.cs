using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;
using Xunit;

namespace TaskFlow.Tests.Integration.Application.Boards;

[Collection("SequentialTests")]
public class BoardsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly Guid _userId = Guid.Parse(TestAuthHandler.UserId);

    private async Task SeedUser(TaskFlowDbContext context)
    {
        if (!await context.Users.AnyAsync(u => u.Id == _userId))
        {
            context.Users.Add(new User { Id = _userId, Email = "test@test.com", UserName = "Tester", Role = UserRole.User, PasswordHash = "hash"});
            await context.SaveChangesAsync();
        }
    }
    
    [Fact]
    public async Task CreateBoardCommand_WritesToDatabase()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);
        var command = new CreateBoardCommand(_userId, "Integration", true);

        // Act
        var boardId = await mediator.Send(command);

        // Assert
        var board = await scope.DbContext.Boards.FindAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("Integration", board.Name);
        Assert.True(board.IsPublic);

        var userBoard = await scope.DbContext.UserBoards
            .FirstOrDefaultAsync(ub => ub.BoardId == boardId && ub.UserId == _userId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Owner, userBoard.BoardRole);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsOnlyUserMemberBoards()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var otherUserId = Guid.NewGuid();
        
        await SeedUser(scope.DbContext);
        scope.DbContext.Users.Add(new User { Id = otherUserId, Email = "other@test.com", UserName = "OtherTester", Role = UserRole.User, PasswordHash = "hash"});

        var board1 = new Board { Id = Guid.NewGuid(), Name = "Board1", IsPublic = true };
        var board2 = new Board { Id = Guid.NewGuid(), Name = "Board2", IsPublic = false };
        var board3 = new Board { Id = Guid.NewGuid(), Name = "Board3", IsPublic = false };

        scope.DbContext.Boards.AddRange(board1, board2, board3);
        scope.DbContext.UserBoards.AddRange(
            new UserBoard { UserId = _userId, BoardId = board1.Id, BoardRole = BoardRole.Owner },
            new UserBoard { UserId = _userId, BoardId = board2.Id, BoardRole = BoardRole.Editor },
            new UserBoard { UserId = otherUserId, BoardId = board3.Id, BoardRole = BoardRole.Owner }
        );
        await scope.DbContext.SaveChangesAsync();

        var query = new GetBoardsQuery(_userId);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Id == board1.Id);
        Assert.Contains(result, b => b.Id == board2.Id);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsEmptyListWhenNoBoardsExist()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);
        var query = new GetBoardsQuery(_userId);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateBoardCommand_UpdatesDatabase()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);

        var boardId = Guid.NewGuid();
        scope.DbContext.Boards.Add(new Board { Id = boardId, Name = "Old Name", IsPublic = true });
        scope.DbContext.UserBoards.Add(new UserBoard { UserId = _userId, BoardId = boardId, BoardRole = BoardRole.Owner });
        await scope.DbContext.SaveChangesAsync();

        var command = new UpdateBoardCommand(_userId, boardId, "New Name", false);

        // Act
        await mediator.Send(command);

        // Assert
        var board = await scope.DbContext.Boards.FindAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("New Name", board.Name);
        Assert.False(board.IsPublic);
    }

    [Fact]
    public async Task DeleteBoardCommand_RemovesFromDatabase()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);

        var boardId = Guid.NewGuid();
        scope.DbContext.Boards.Add(new Board { Id = boardId, Name = "To Delete", IsPublic = true });
        scope.DbContext.UserBoards.Add(new UserBoard { UserId = _userId, BoardId = boardId, BoardRole = BoardRole.Owner });
        await scope.DbContext.SaveChangesAsync();

        var command = new DeleteBoardCommand(_userId, boardId);

        // Act
        await mediator.Send(command);

        // Assert
        var board = await scope.DbContext.Boards.FindAsync(boardId);
        Assert.Null(board);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ReturnsCorrectBoard()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);

        var boardId = Guid.NewGuid();
        scope.DbContext.Boards.Add(new Board { Id = boardId, Name = "Test Board", IsPublic = true });
        scope.DbContext.UserBoards.Add(new UserBoard { UserId = _userId, BoardId = boardId, BoardRole = BoardRole.Viewer });
        await scope.DbContext.SaveChangesAsync();

        var query = new GetBoardByIdQuery(_userId, boardId);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result.Id);
        Assert.Equal("Test Board", result.Name);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ThrowsNotFoundException_WhenBoardDoesNotExist()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);
        var query = new GetBoardByIdQuery(_userId, Guid.NewGuid());

        // Act
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => mediator.Send(query));
    }
    
    [Fact]
    public async Task UpdateBoardCommand_ThrowsForbiddenException_WhenUserIsNotOwner()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        await SeedUser(scope.DbContext);

        var boardId = Guid.NewGuid();
        scope.DbContext.Boards.Add(new Board { Id = boardId, Name = "Old Name", IsPublic = false });
        scope.DbContext.UserBoards.Add(new UserBoard { UserId = _userId, BoardId = boardId, BoardRole = BoardRole.Editor });
        await scope.DbContext.SaveChangesAsync();

        var command = new UpdateBoardCommand(_userId, boardId, "New Name", true);

        // Act
        // Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => mediator.Send(command));
    }
}