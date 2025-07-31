using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Handlers;
using TaskFlow.Application.Boards.Queries;

namespace TaskFlow.Tests.Integration.Application.Boards;

public class BoardIntegrationTests : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private readonly IMapper _mapper;
        
    public BoardIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    {
        using var scope = factory.Services.CreateScope();
        _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    }
        
    private TaskFlowDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TaskFlowDbContext(options);
    }

    [Fact]
    public async Task CreateBoardCommand_WritesToDatabase()
    {
        using var context = GetDbContext();
        var uow = new UnitOfWork(context);
        var handler = new CreateBoardHandler(uow);
        var command = new CreateBoardCommand("Integration", true);

        var boardId = await handler.Handle(command, default);

        var board = await context.Boards.FindAsync(boardId);
        Assert.NotNull(board);
        Assert.Equal("Integration", board.Name);
        Assert.True(board.IsPublic);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsAllBoards()
    {
        using var context = GetDbContext();
        context.Boards.Add(new Domain.Entities.Board { Name = "Board1", IsPublic = true });
        context.Boards.Add(new Domain.Entities.Board { Name = "Board2", IsPublic = false });
        await context.SaveChangesAsync();

        var uow = new UnitOfWork(context);
        var handler = new GetBoardsHandler(uow, _mapper);

        var boards = await handler.Handle(new GetBoardsQuery(), default);

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
        using var context = GetDbContext();
        var board = new Domain.Entities.Board { Name = "OldName", IsPublic = false };
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var uow = new UnitOfWork(context);
        var handler = new UpdateBoardHandler(uow);
        var command = new UpdateBoardCommand(board.Id, "NewName", true);

        await handler.Handle(command, default);

        var updatedBoard = await context.Boards.FindAsync(board.Id);
        Assert.NotNull(updatedBoard);
        Assert.Equal("NewName", updatedBoard.Name);
        Assert.True(updatedBoard.IsPublic);
    }
        
    [Fact]
    public async Task DeleteBoardCommand_RemovesBoardFromDatabase()
    {
        using var context = GetDbContext();
        var board = new Domain.Entities.Board { Name = "ToDelete", IsPublic = true };
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var uow = new UnitOfWork(context);
        var handler = new DeleteBoardHandler(uow);

        await handler.Handle(new DeleteBoardCommand(board.Id), default);

        var deletedBoard = await context.Boards.FindAsync(board.Id);
        Assert.Null(deletedBoard);
    }
        
    [Fact]
    public async Task GetBoardByIdQuery_ReturnsCorrectBoard()
    {
        using var context = GetDbContext();
        var board = new Domain.Entities.Board { Name = "TestBoard", IsPublic = true };
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var uow = new UnitOfWork(context);
        var handler = new GetBoardByIdHandler(uow, _mapper);

        var result = await handler.Handle(new GetBoardByIdQuery(board.Id), default);

        Assert.NotNull(result);
        Assert.Equal(board.Id, result.Id);
        Assert.Equal("TestBoard", result.Name);
        Assert.True(result.IsPublic);
    }

    [Fact]
    public async Task GetBoardByIdQuery_ReturnsNullForNonExistentBoard()
    {
        using var context = GetDbContext();
        var uow = new UnitOfWork(context);
        var handler = new GetBoardByIdHandler(uow, _mapper);

        var result = await handler.Handle(new GetBoardByIdQuery(Guid.NewGuid()), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardsQuery_ReturnsEmptyListWhenNoBoardsExist()
    {
        using var context = GetDbContext();
        var uow = new UnitOfWork(context);
        var handler = new GetBoardsHandler(uow, _mapper);

        var result = await handler.Handle(new GetBoardsQuery(), default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}