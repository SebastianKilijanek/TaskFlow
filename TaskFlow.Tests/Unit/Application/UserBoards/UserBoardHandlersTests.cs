using AutoMapper;
using Moq;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Application.UserBoards.DTO;
using TaskFlow.Application.UserBoards.Handlers;
using TaskFlow.Application.UserBoards.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Unit.Application.UserBoards;

public class UserBoardHandlersTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<UserBoard>> _userBoardRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    public UserBoardHandlersTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<UserBoard>()).Returns(_userBoardRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task AddUserToBoardHandler_Should_AddUser()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userToAdd = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var board = new Board { Id = boardId, Name = "Test Board", UserBoards = new List<UserBoard>() };
        var requesterUserBoard = new UserBoard { UserId = TestSeeder.DefaultUserId, BoardId = boardId, BoardRole = BoardRole.Owner };
        
        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(TestSeeder.DefaultUserId, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requesterUserBoard);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(userToAdd.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userToAdd);

        var handler = new AddUserToBoardHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        var command = new AddUserToBoardCommand(TestSeeder.DefaultUserId, boardId, userToAdd.Email, BoardRole.Editor) { Board = board };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userBoardRepositoryMock.Verify(r => r.AddAsync(It.Is<UserBoard>(ub =>
            ub.UserId == userToAdd.Id &&
            ub.BoardId == boardId &&
            ub.BoardRole == BoardRole.Editor
        ), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendAsync(userToAdd.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUserFromBoardHandler_Should_RemoveUser()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userIdToRemove = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Test Board" };
        var userToRemove = new User { Id = userIdToRemove, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var requesterUserBoard = new UserBoard { UserId = TestSeeder.DefaultUserId, BoardId = boardId, BoardRole = BoardRole.Owner };
        var userBoardToRemove = new UserBoard { UserId = userIdToRemove, BoardId = boardId, BoardRole = BoardRole.Editor, User = userToRemove, Board = board };

        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(TestSeeder.DefaultUserId, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requesterUserBoard);
        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(userIdToRemove, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBoardToRemove);

        var handler = new RemoveUserFromBoardHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        var command = new RemoveUserFromBoardCommand(TestSeeder.DefaultUserId, boardId, userIdToRemove);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userBoardRepositoryMock.Verify(r => r.Remove(userBoardToRemove), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendAsync(userToRemove.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ChangeUserBoardRoleHandler_Should_UpdateRole()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userIdToChange = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Test Board" };
        var userToChange = new User { Id = userIdToChange, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var userBoardToChange = new UserBoard { UserId = userIdToChange, BoardId = boardId, BoardRole = BoardRole.Viewer, User = userToChange, Board = board };
        
        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(userIdToChange, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBoardToChange);

        var handler = new ChangeUserBoardRoleHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        var command = new ChangeUserBoardRoleCommand(TestSeeder.DefaultUserId, boardId, userIdToChange, BoardRole.Editor);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(BoardRole.Editor, userBoardToChange.BoardRole);
        _userBoardRepositoryMock.Verify(r => r.Update(userBoardToChange), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendAsync(userToChange.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task TransferBoardOwnershipHandler_Should_TransferOwnership()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var board = new Board { Id = boardId, Name = "Test Board" };
        var newOwner = new User { Id = newOwnerId, Email = "test@test.com", UserName = "Tester", PasswordHash = "hash" };
        var currentOwnerUserBoard = new UserBoard { UserId = TestSeeder.DefaultUserId, BoardId = boardId, BoardRole = BoardRole.Owner };
        var newOwnerUserBoard = new UserBoard { UserId = newOwnerId, BoardId = boardId, BoardRole = BoardRole.Editor, User = newOwner, Board = board };
        
        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(TestSeeder.DefaultUserId, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentOwnerUserBoard);
        _userBoardRepositoryMock.Setup(r => r.GetByIdAsync(newOwnerId, boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOwnerUserBoard);

        var handler = new TransferBoardOwnershipHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        var command = new TransferBoardOwnershipCommand(TestSeeder.DefaultUserId, boardId, newOwnerId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(BoardRole.Editor, currentOwnerUserBoard.BoardRole);
        Assert.Equal(BoardRole.Owner, newOwnerUserBoard.BoardRole);
        _userBoardRepositoryMock.Verify(r => r.Update(currentOwnerUserBoard), Times.Once);
        _userBoardRepositoryMock.Verify(r => r.Update(newOwnerUserBoard), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendAsync(newOwner.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersInBoardHandler_Should_ReturnUsers()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userBoards = new List<UserBoard>
        {
            new() { UserId = TestSeeder.DefaultUserId, BoardId = boardId, BoardRole = BoardRole.Owner },
            new() { UserId = Guid.NewGuid(), BoardId = boardId, BoardRole = BoardRole.Editor }
        };
        var userBoardDTOs = new List<UserBoardDTO>
        {
            new() { UserId = userBoards[0].UserId, BoardId = boardId, BoardRole = "Owner", JoinedAt = DateTime.UtcNow },
            new() { UserId = userBoards[1].UserId, BoardId = boardId, BoardRole = "Editor", JoinedAt = DateTime.UtcNow }
        };

        _userBoardRepositoryMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<UserBoard, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBoards);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<UserBoardDTO>>(userBoards)).Returns(userBoardDTOs);

        var handler = new GetUsersInBoardHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var query = new GetUsersInBoardQuery(TestSeeder.DefaultUserId, boardId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(userBoardDTOs, result);
    }
}