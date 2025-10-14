using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.API.Comments;

[Collection("SequentialTests")]
public class CommentsAPIIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string COMMENTS_BASE_URL = "/api/comments";

    [Fact]
    public async Task Post_AddComment_Returns201()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var payload = new AddCommentDTO { TaskItemId = taskItemId, Content = "My new comment" };

        // Act
        var response = await client.PostAsJsonAsync(COMMENTS_BASE_URL, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var commentId = Guid.Parse(location.Segments.Last());
        var comment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.NotNull(comment);
        Assert.Equal("My new comment", comment.Content);
        Assert.Equal(taskItemId, comment.TaskItemId);
    }

    [Fact]
    public async Task Get_CommentsForTask_Returns200WithComments()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        await TestSeeder.SeedComment(testScope, taskItemId, "Comment 1");
        await TestSeeder.SeedComment(testScope, taskItemId, "Comment 2");

        // Act
        var response = await client.GetAsync($"{COMMENTS_BASE_URL}/taskitem/{taskItemId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var comments = await response.Content.ReadFromJsonAsync<List<CommentDTO>>();
        Assert.NotNull(comments);
        Assert.Equal(2, comments.Count);
    }

    [Fact]
    public async Task Get_CommentById_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId, "Test Comment");

        // Act
        var response = await client.GetAsync($"{COMMENTS_BASE_URL}/{commentId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var comment = await response.Content.ReadFromJsonAsync<CommentDTO>();
        Assert.NotNull(comment);
        Assert.Equal("Test Comment", comment.Content);
    }

    [Fact]
    public async Task Put_UpdateComment_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId, "Old Content");
        var payload = new UpdateCommentDTO { CommentId = commentId, Content = "New Content" };

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PutAsJsonAsync($"{COMMENTS_BASE_URL}/{commentId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedComment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.NotNull(updatedComment);
        Assert.Equal("New Content", updatedComment.Content);
    }

    [Fact]
    public async Task Delete_Comment_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();
        var boardId = await TestSeeder.SeedBoard(testScope);
        var columnId = await TestSeeder.SeedColumn(testScope, boardId);
        var taskItemId = await TestSeeder.SeedTask(testScope, columnId);
        var commentId = await TestSeeder.SeedComment(testScope, taskItemId);

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.DeleteAsync($"{COMMENTS_BASE_URL}/{commentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedComment = await testScope.UnitOfWork.Repository<Comment>().GetByIdAsync(commentId);
        Assert.Null(deletedComment);
    }
}