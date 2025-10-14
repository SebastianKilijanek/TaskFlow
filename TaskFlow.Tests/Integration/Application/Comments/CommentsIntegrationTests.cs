using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Comments.Queries;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Application.Comments;

[Collection("SequentialTests")]
public class CommentsIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task AddCommentCommand_WritesToDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var command = new AddCommentCommand(TestSeeder.DefaultUserId, taskItemId, "Integration Test Comment");

        // Act
        var commentId = await testScope.Mediator.Send(command);

        // Assert
        var comment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.NotNull(comment);
        Assert.Equal("Integration Test Comment", comment.Content);
        Assert.Equal(taskItemId, comment.TaskItemId);
        Assert.Equal(TestSeeder.DefaultUserId, comment.AuthorId);
    }

    [Fact]
    public async Task DeleteCommentCommand_RemovesFromDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId, "To Delete", TestSeeder.DefaultUserId);
        var command = new DeleteCommentCommand(TestSeeder.DefaultUserId, commentId);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var deletedComment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.Null(deletedComment);
    }

    [Fact]
    public async Task UpdateCommentCommand_UpdatesExistingComment()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId, "Old Content", TestSeeder.DefaultUserId);
        var command = new UpdateCommentCommand(TestSeeder.DefaultUserId, commentId, "New Content");

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var updatedComment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.NotNull(updatedComment);
        Assert.Equal("New Content", updatedComment.Content);
    }

    [Fact]
    public async Task GetCommentByIdQuery_ReturnsCorrectComment()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId, "Get Me By Id", TestSeeder.DefaultUserId);
        var query = new GetCommentByIdQuery(TestSeeder.DefaultUserId, commentId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(commentId, result.Id);
        Assert.Equal("Get Me By Id", result.Content);
    }

    [Fact]
    public async Task GetCommentsForTaskQuery_ReturnsAllCommentsForTask()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        await TestSeeder.SeedComment(testScope, taskItemId, "First", TestSeeder.DefaultUserId);
        await TestSeeder.SeedComment(testScope, taskItemId, "Second", TestSeeder.DefaultUserId);
        var query = new GetCommentsForTaskQuery(TestSeeder.DefaultUserId, taskItemId);

        // Act
        var result = (await testScope.Mediator.Send(query)).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Content == "First");
        Assert.Contains(result, c => c.Content == "Second");
    }
}