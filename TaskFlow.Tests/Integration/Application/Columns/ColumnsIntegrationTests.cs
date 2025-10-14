using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Application.Columns;

[Collection("SequentialTests")]
public class ColumnsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task CreateColumnCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false);
        var command = new CreateColumnCommand(TestSeeder.DefaultUserId, "Test Column", boardId);

        // Act
        var columnId = await testScope.Mediator.Send(command);

        // Assert
        var column = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId);
        Assert.NotNull(column);
        Assert.Equal("Test Column", column.Name);
        Assert.Equal(boardId, column.BoardId);
        Assert.Equal(0, column.Position);
    }

    [Fact]
    public async Task UpdateColumnCommand_UpdatesExistingColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId, "Old Column");
        var command = new UpdateColumnCommand(TestSeeder.DefaultUserId, columnId, "New Column");

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var updatedColumn = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(columnId);
        Assert.NotNull(updatedColumn);
        Assert.Equal("New Column", updatedColumn.Name);
    }

    [Fact]
    public async Task DeleteColumnCommand_RemovesColumnAndReorders()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false);
        var col1Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 1");
        var col2Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 2", 1);
        var col3Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 3", 2);
        var command = new DeleteColumnCommand(TestSeeder.DefaultUserId, col2Id);

        // Act
         await testScope.Mediator.Send(command);

        // Assert
        var deleted = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col2Id);
        Assert.Null(deleted);
        var unchangedCol1 = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col1Id);
        Assert.NotNull(unchangedCol1);
        Assert.Equal(0, unchangedCol1.Position);
        var reorderedCol3 = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col3Id);
        Assert.NotNull(reorderedCol3);
        Assert.Equal(1, reorderedCol3.Position);
    }

    [Fact]
    public async Task GetColumnByIdQuery_ReturnsCorrectColumn()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false, role: BoardRole.Viewer);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        
        var query = new GetColumnByIdQuery(TestSeeder.DefaultUserId, columnId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(columnId, result.Id);
        Assert.Equal("Test Column", result.Name);
    }

    [Fact]
    public async Task GetColumnsByBoardQuery_ReturnsColumnsForBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false, role: BoardRole.Viewer);
        await TestSeeder.SeedColumn(testScope, boardId, "Column 1", 0);
        await TestSeeder.SeedColumn(testScope, boardId, "Column 2", 1);
        var query = new GetColumnsByBoardQuery(TestSeeder.DefaultUserId, boardId);

        // Act
        var result = await testScope.Mediator.Send(query);

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
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false);
        var col1Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 1");
        var col2Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 2", 1);
        var col3Id = await TestSeeder.SeedColumn(testScope, boardId, "Column 3", 2);
        var command = new MoveColumnCommand(TestSeeder.DefaultUserId, col1Id, 2);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var movedCol = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col1Id);
        Assert.NotNull(movedCol);
        Assert.Equal(2, movedCol.Position);

        var otherCol2 = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col2Id);
        Assert.NotNull(otherCol2);
        Assert.Equal(0, otherCol2.Position);
        
        var otherCol3 = await testScope.UnitOfWork.Repository<Column>().GetByIdAsync(col3Id);
        Assert.NotNull(otherCol3);
        Assert.Equal(1, otherCol3.Position);
    }
    
    [Fact]
    public async Task UpdateColumnCommand_ThrowsForbiddenException_WhenUserIsNotEditorOrOwner()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope, isPublic: false, role: BoardRole.Viewer);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId, "Old Column");
        var command = new UpdateColumnCommand(TestSeeder.DefaultUserId, columnId, "New Column");

        // Act
        // Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => testScope.Mediator.Send(command));
    }
}