using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests;

public static class TestSeeder
{
    public static readonly Guid DefaultUserId = Guid.Parse(TestAuthHandler.UserId);

    public static async Task<User> SeedUser(TestScope scope, Guid? userId = null, string email = "test@test.com",
        string username = "Tester", string password = "Password123!", bool hashPassword = false)
    {
        var id = userId ?? DefaultUserId;
        var user = new User
        {
            Id = id,
            Email = email,
            UserName = username,
            PasswordHash = password,
            Role = UserRole.User
        };
        
        if (hashPassword)
        {
            var passwordHasher = scope.ServiceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }
    
        await scope.UnitOfWork.Repository<User>().AddAsync(user);
        await scope.UnitOfWork.SaveChangesAsync();
        return user;
    }

    public static async Task<Guid> SeedBoard(TestScope scope, Guid? ownerId = null, string name = "Test Board", bool isPublic = true, BoardRole role = BoardRole.Owner)
    {
        var boardId = Guid.NewGuid();
        var userId = ownerId ?? DefaultUserId;
        var board = new Board { Id = boardId, Name = name, IsPublic = isPublic };
        var userBoard = new UserBoard { UserId = userId, BoardId = boardId, BoardRole = role };

        await scope.UnitOfWork.Repository<Board>().AddAsync(board);
        await scope.UnitOfWork.Repository<UserBoard>().AddAsync(userBoard);
        await scope.UnitOfWork.SaveChangesAsync();
        
        return boardId;
    }

    public static async Task<Guid> SeedColumn(TestScope scope, Guid boardId, string name = "Test Column", int position = 0)
    {
        var columnId = Guid.NewGuid();
        var column = new Column { Id = columnId, Name = name, Position = position, BoardId = boardId };
        
        await scope.UnitOfWork.Repository<Column>().AddAsync(column);
        await scope.UnitOfWork.SaveChangesAsync();
        
        return columnId;
    }

    public static async Task<Guid> SeedTask(TestScope scope, Guid columnId, string title = "Test Task", int position = 0)
    {
        var taskItemId = Guid.NewGuid();
        var taskItem = new TaskItem
        {
            Id = taskItemId,
            Title = title,
            Description = "Description",
            Position = position,
            Status = TaskItemStatus.ToDo,
            ColumnId = columnId,
            CreatedAt = DateTime.UtcNow
        };
        
        await scope.UnitOfWork.Repository<TaskItem>().AddAsync(taskItem);
        await scope.UnitOfWork.SaveChangesAsync();
        
        return taskItemId;
    }

    public static async Task<Guid> SeedComment(TestScope scope, Guid taskItemId, string content = "Test Comment", Guid authorId = default)
    {
        if (authorId == Guid.Empty)
            authorId = DefaultUserId;
        
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = commentId,
            Content = content,
            TaskItemId = taskItemId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };
        
        await scope.UnitOfWork.Repository<Comment>().AddAsync(comment);
        await scope.UnitOfWork.SaveChangesAsync();
        
        return commentId;
    }
}