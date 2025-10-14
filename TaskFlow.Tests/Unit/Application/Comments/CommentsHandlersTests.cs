using System.Linq.Expressions;
using AutoMapper;
using Moq;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.Comments.Handlers;
using TaskFlow.Application.Comments.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.Comments;

public class CommentsHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IRepository<Comment>> _commentRepositoryMock = new();

    public CommentsHandlersTests()
    {
        _unitOfWorkMock.Setup(uow => uow.Repository<Comment>()).Returns(_commentRepositoryMock.Object);
    }

    [Fact]
    public async Task AddCommentHandler_Should_AddComment()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var handler = new AddCommentHandler(_unitOfWorkMock.Object);
        var command = new AddCommentCommand(TestSeeder.DefaultUserId, taskItemId, "New Comment");

        // Act
        var commentId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, commentId);
        _commentRepositoryMock.Verify(r => r.AddAsync(It.Is<Comment>(c =>
            c.Id == commentId &&
            c.Content == "New Comment" &&
            c.TaskItemId == taskItemId &&
            c.AuthorId == TestSeeder.DefaultUserId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentHandler_Should_RemoveComment()
    {
        // Arrange
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "To Delete",
            TaskItemId = Guid.NewGuid(),
            AuthorId = TestSeeder.DefaultUserId,
            CreatedAt = DateTime.UtcNow
        };
        var handler = new DeleteCommentHandler(_unitOfWorkMock.Object);
        var command = new DeleteCommentCommand(TestSeeder.DefaultUserId, comment.Id) { Entity = comment };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(r => r.Remove(comment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCommentByIdHandler_Should_ReturnCorrectComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = commentId, 
            Content = "Test Comment",
            TaskItemId = Guid.NewGuid(), 
            AuthorId = TestSeeder.DefaultUserId,
            CreatedAt = DateTime.UtcNow
        };
        var commentDTO = new CommentDTO
        {
            Id = commentId,
            Content = "Test Comment",
            TaskItemId = comment.TaskItemId,
            AuthorId = TestSeeder.DefaultUserId,
            AuthorUserName = "TestUser",
            CreatedAt = comment.CreatedAt
        };
        _mapperMock.Setup(m => m.Map<CommentDTO>(comment)).Returns(commentDTO);
        var handler = new GetCommentByIdHandler(_mapperMock.Object);
        var query = new GetCommentByIdQuery(TestSeeder.DefaultUserId, commentId) { Entity = comment };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(commentId, result.Id);
        Assert.Equal("Test Comment", result.Content);
    }

    [Fact]
    public async Task GetCommentsForTaskHandler_Should_ReturnCommentsForTask()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new()
            {
                Id = Guid.NewGuid(), Content = "Comment 1", TaskItemId = taskItemId, 
                AuthorId = TestSeeder.DefaultUserId, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), Content = "Comment 2", TaskItemId = taskItemId, 
                AuthorId = TestSeeder.DefaultUserId, CreatedAt = DateTime.UtcNow
            }
        };
        var commentDTOs = new List<CommentDTO>
        {
            new()
            {
                Id = comments[0].Id, Content = "Comment 1", TaskItemId = taskItemId, 
                AuthorId = TestSeeder.DefaultUserId, AuthorUserName = "TestUser", CreatedAt = comments[0].CreatedAt
            },
            new()
            {
                Id = comments[1].Id, Content = "Comment 2", TaskItemId = taskItemId, 
                AuthorId = TestSeeder.DefaultUserId, AuthorUserName = "TestUser", CreatedAt = comments[1].CreatedAt
            }
        }.AsReadOnly();

        _unitOfWorkMock.Setup(u => u.Repository<Comment>().ListAsync(It.IsAny<Expression<Func<Comment, bool>>>(), 
                    It.IsAny<CancellationToken>())).ReturnsAsync(comments);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<CommentDTO>>(comments)).Returns(commentDTOs);

        var handler = new GetCommentsForTaskHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetCommentsForTaskQuery(TestSeeder.DefaultUserId, taskItemId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Content == "Comment 1");
    }

    [Fact]
    public async Task UpdateCommentHandler_Should_ChangeContent()
    {
        // Arrange
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "Old Content",
            TaskItemId = Guid.NewGuid(),
            AuthorId = TestSeeder.DefaultUserId,
            CreatedAt = DateTime.UtcNow
        };
        var handler = new UpdateCommentHandler(_unitOfWorkMock.Object);
        var command = new UpdateCommentCommand(TestSeeder.DefaultUserId, comment.Id, "New Content") { Entity = comment };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Content", comment.Content);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}