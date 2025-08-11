using Xunit;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.Handlers;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests.Integration.Application.Columns;

[Collection("SequentialTests")]
public class ColumnsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateColumnCommand_WritesToDatabase()
    {
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", IsPublic = true };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new CreateColumnHandler(testScope.UnitOfWork);
        var command = new CreateColumnCommand("Col1", board.Id, 1);

        var columnId = await handler.Handle(command, default);

        var column = await testScope.DbContext.Columns.FindAsync(columnId);
        Assert.NotNull(column);
        Assert.Equal("Col1", column.Name);
        Assert.Equal(board.Id, column.BoardId);
        Assert.Equal(1, column.Position);
    }

    [Fact]
    public async Task UpdateColumnCommand_UpdatesExistingColumn()
    {
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Old", BoardId = board.Id, Position = 1 };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new UpdateColumnHandler(testScope.UnitOfWork);
        var command = new UpdateColumnCommand(column.Id, "New", 2);

        await handler.Handle(command, default);

        var updated = await testScope.DbContext.Columns.FindAsync(column.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.Name);
        Assert.Equal(2, updated.Position);
    }

    [Fact]
    public async Task DeleteColumnCommand_RemovesColumnFromDatabase()
    {
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "ToDelete", BoardId = board.Id, Position = 1 };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new DeleteColumnHandler(testScope.UnitOfWork);
        var command = new DeleteColumnCommand(column.Id);

        await handler.Handle(command, default);

        var deleted = await testScope.DbContext.Columns.FindAsync(column.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetColumnByIdQuery_ReturnsCorrectColumn()
    {
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Col", BoardId = board.Id, Position = 1 };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetColumnByIdHandler(testScope.UnitOfWork, testScope.Mapper);
        var query = new GetColumnByIdQuery(column.Id);

        var result = await handler.Handle(query, default);

        Assert.NotNull(result);
        Assert.Equal(column.Id, result.Id);
        Assert.Equal("Col", result.Name);
        Assert.Equal(1, result.Position);
    }

    [Fact]
    public async Task GetColumnsByBoardQuery_ReturnsColumnsForBoard()
    {
        var testScope = factory.GetTestScope();
        var board1 = new Board { Id = Guid.NewGuid(), Name = "Board1", IsPublic = true };
        var board2 = new Board { Id = Guid.NewGuid(), Name = "Board2", IsPublic = false };
        var col1 = new Column { Id = Guid.NewGuid(), Name = "Col1", BoardId = board1.Id, Position = 1 };
        var col2 = new Column { Id = Guid.NewGuid(), Name = "Col2", BoardId = board1.Id, Position = 2 };
        var col3 = new Column { Id = Guid.NewGuid(), Name = "Col3", BoardId = board2.Id, Position = 1 };
        await testScope.DbContext.AddRangeAsync(board1, board2, col1, col2, col3);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new GetColumnsByBoardHandler(testScope.UnitOfWork, testScope.Mapper);
        var query = new GetColumnsByBoardQuery(board1.Id);

        var result = (await handler.Handle(query, default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == col1.Id);
        Assert.Contains(result, c => c.Id == col2.Id);
        Assert.DoesNotContain(result, c => c.Id == col3.Id);
    }

    [Fact]
    public async Task MoveColumnCommand_UpdatesPosition()
    {
        var testScope = factory.GetTestScope();
        var board = new Board { Id = Guid.NewGuid(), Name = "Board", IsPublic = true };
        var column = new Column { Id = Guid.NewGuid(), Name = "Col", BoardId = board.Id, Position = 1 };
        await testScope.DbContext.Boards.AddAsync(board);
        await testScope.DbContext.Columns.AddAsync(column);
        await testScope.DbContext.SaveChangesAsync();

        var handler = new MoveColumnHandler(testScope.UnitOfWork);
        var command = new MoveColumnCommand(column.Id, 5);

        await handler.Handle(command, default);

        var moved = await testScope.DbContext.Columns.FindAsync(column.Id);
        Assert.NotNull(moved);
        Assert.Equal(5, moved.Position);
    }
}