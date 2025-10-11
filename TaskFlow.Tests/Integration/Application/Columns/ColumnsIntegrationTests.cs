using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;
using Xunit;

namespace TaskFlow.Tests.Integration.Application.Columns;

[Collection("SequentialTests")]
public class ColumnsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly Guid _userId = Guid.Parse(TestAuthHandler.UserId);

    private async Task<Guid> SeedUserAndBoard(TaskFlowDbContext context, BoardRole userRole = BoardRole.Owner)
    {
        if (!await context.Users.AnyAsync(u => u.Id == _userId))
        {
            context.Users.Add(new User { Id = _userId, Email = "test@test.com", UserName = "Tester", Role = UserRole.User, PasswordHash = "hash"});
        }

        var board = new Board { Id = Guid.NewGuid(), Name = "Test Board", IsPublic = false };
        context.Boards.Add(board);
        context.UserBoards.Add(new UserBoard { UserId = _userId, BoardId = board.Id, BoardRole = userRole });
        
        await context.SaveChangesAsync();
        
        return board.Id;
    }

    [Fact]
    public async Task CreateColumnCommand_WritesToDatabase()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await SeedUserAndBoard(scope.DbContext);
        var command = new CreateColumnCommand(_userId, "Test Column", boardId);

        // Act
        var columnId = await mediator.Send(command);

        // Assert
        var column = await scope.DbContext.Columns.FindAsync(columnId);
        Assert.NotNull(column);
        Assert.Equal("Test Column", column.Name);
        Assert.Equal(boardId, column.BoardId);
        Assert.Equal(0, column.Position);
    }

    [Fact]
    public async Task UpdateColumnCommand_UpdatesExistingColumn()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId =  await SeedUserAndBoard(scope.DbContext);
        var column = new Column { Id = Guid.NewGuid(), Name = "Old Column", BoardId = boardId, Position = 0 };
        scope.DbContext.Columns.Add(column);
        await scope.DbContext.SaveChangesAsync();
        var command = new UpdateColumnCommand(_userId, column.Id, "New Column");

        // Act
        await mediator.Send(command);

        // Assert
        var updatedColumn = await scope.DbContext.Columns.FindAsync(column.Id);
        Assert.NotNull(updatedColumn);
        Assert.Equal("New Column", updatedColumn.Name);
    }

    [Fact]
    public async Task DeleteColumnCommand_RemovesColumnAndReorders()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId =  await SeedUserAndBoard(scope.DbContext);
        var col1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = boardId, Position = 0 };
        var col2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", BoardId = boardId, Position = 1 };
        var col3 = new Column { Id = Guid.NewGuid(), Name = "Column 3", BoardId = boardId, Position = 2 };
        scope.DbContext.Columns.AddRange(col1, col2, col3);
        await scope.DbContext.SaveChangesAsync();
        var command = new DeleteColumnCommand(_userId, col2.Id);

        // Act
         await mediator.Send(command);

        // Assert
        var deleted = await scope.DbContext.Columns.FindAsync(col2.Id);
        Assert.Null(deleted);
        var reorderedCol3 = await scope.DbContext.Columns.FindAsync(col3.Id);
        Assert.NotNull(reorderedCol3);
        Assert.Equal(1, reorderedCol3.Position);
    }

    [Fact]
    public async Task GetColumnByIdQuery_ReturnsCorrectColumn()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await SeedUserAndBoard(scope.DbContext, BoardRole.Viewer);
        var column = new Column { Id = Guid.NewGuid(), Name = "Test Column", BoardId = boardId, Position = 0 };
        scope.DbContext.Columns.Add(column);
        await scope.DbContext.SaveChangesAsync();
        var query = new GetColumnByIdQuery(_userId, column.Id);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(column.Id, result.Id);
        Assert.Equal("Test Column", result.Name);
    }

    [Fact]
    public async Task GetColumnsByBoardQuery_ReturnsColumnsForBoard()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await SeedUserAndBoard(scope.DbContext, BoardRole.Viewer);
        var col1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = boardId, Position = 0 };
        var col2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", BoardId = boardId, Position = 1 };
        scope.DbContext.Columns.AddRange(col1, col2);
        await scope.DbContext.SaveChangesAsync();
        var query = new GetColumnsByBoardQuery(_userId, boardId);

        // Act
        var result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Column 1");
        Assert.Contains(result, c => c.Name == "Column 2");
    }

    [Fact]
    public async Task MoveColumnCommand_UpdatesPositionAndReorders()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await SeedUserAndBoard(scope.DbContext);
        var col1 = new Column { Id = Guid.NewGuid(), Name = "Column 1", BoardId = boardId, Position = 0 };
        var col2 = new Column { Id = Guid.NewGuid(), Name = "Column 2", BoardId = boardId, Position = 1 };
        var col3 = new Column { Id = Guid.NewGuid(), Name = "Column 3", BoardId = boardId, Position = 2 };
        scope.DbContext.Columns.AddRange(col1, col2, col3);
        await scope.DbContext.SaveChangesAsync();
        var command = new MoveColumnCommand(_userId, col1.Id, 2);

        // Act
        await mediator.Send(command);

        // Assert
        var movedCol = await scope.DbContext.Columns.FindAsync(col1.Id);
        Assert.NotNull(movedCol);
        Assert.Equal(2, movedCol.Position);

        var otherCol2 = await scope.DbContext.Columns.FindAsync(col2.Id);
        Assert.NotNull(otherCol2);
        Assert.Equal(0, otherCol2.Position);
        
        var otherCol3 = await scope.DbContext.Columns.FindAsync(col3.Id);
        Assert.NotNull(otherCol3);
        Assert.Equal(1, otherCol3.Position);
    }
    
    [Fact]
    public async Task UpdateColumnCommand_ThrowsForbiddenException_WhenUserIsNotEditorOrOwner()
    {
        // Arrange
        var scope = factory.GetTestScope();
        var mediator = scope.ServiceScope.ServiceProvider.GetRequiredService<IMediator>();
        var boardId = await SeedUserAndBoard(scope.DbContext, BoardRole.Viewer);
        var column = new Column { Id = Guid.NewGuid(), Name = "Old Column", BoardId = boardId, Position = 0 };
        scope.DbContext.Columns.Add(column);
        await scope.DbContext.SaveChangesAsync();
        var command = new UpdateColumnCommand(_userId, column.Id, "New Column");

        // Act
        // Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => mediator.Send(command));
    }
}