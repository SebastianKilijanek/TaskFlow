using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Application.UserBoards.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Integration.Application.UserBoards;

[Collection("SequentialTests")]
public class UserBoardIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task AddUserToBoardCommand_AddsUserToBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var user = await TestSeeder.SeedUser(testScope, Guid.NewGuid(), email: "newtest@test.com", username: "NewTester");
        var command = new AddUserToBoardCommand(TestSeeder.DefaultUserId, boardId, user.Email, BoardRole.Editor);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var userBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(user.Id, boardId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Editor, userBoard.BoardRole);
        Assert.Equal("NewTester", userBoard.User.UserName);
    }

    [Fact]
    public async Task RemoveUserFromBoardCommand_RemovesUserFromBoard()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var userToRemove = await TestSeeder.SeedUser(testScope, Guid.NewGuid(), email: "removetest@test.com", username: "RemoveTester");
        await TestSeeder.SeedUserBoard(testScope, userToRemove.Id, boardId, BoardRole.Editor);
        
        var command = new RemoveUserFromBoardCommand(TestSeeder.DefaultUserId, boardId, userToRemove.Id);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var deletedUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(userToRemove.Id, boardId);
        Assert.Null(deletedUserBoard);
    }

    [Fact]
    public async Task ChangeUserBoardRoleCommand_ChangesRole()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var userToChange = await TestSeeder.SeedUser(testScope, Guid.NewGuid(), email: "changetest@.test.com", username: "ChangeTester");
        await TestSeeder.SeedUserBoard(testScope, userToChange.Id, boardId, BoardRole.Viewer);
        
        var command = new ChangeUserBoardRoleCommand(TestSeeder.DefaultUserId, boardId, userToChange.Id, BoardRole.Editor);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var updatedUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(userToChange.Id, boardId);
        Assert.NotNull(updatedUserBoard);
        Assert.Equal(BoardRole.Editor, updatedUserBoard.BoardRole);
    }

    [Fact]
    public async Task TransferBoardOwnershipCommand_TransfersOwnership()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var newOwner = await TestSeeder.SeedUser(testScope, Guid.NewGuid(), email: "newownertest@.test.com", username: "NewOwner");
        await TestSeeder.SeedUserBoard(testScope, newOwner.Id, boardId, BoardRole.Editor);
        
        var command = new TransferBoardOwnershipCommand(TestSeeder.DefaultUserId, boardId, newOwner.Id);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var oldOwnerUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(TestSeeder.DefaultUserId, boardId);
        var updatedNewOwnerUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(newOwner.Id, boardId);
        Assert.NotNull(oldOwnerUserBoard);
        Assert.NotNull(updatedNewOwnerUserBoard);
        Assert.Equal(BoardRole.Editor, oldOwnerUserBoard.BoardRole);
        Assert.Equal(BoardRole.Owner, updatedNewOwnerUserBoard.BoardRole);
    }

    [Fact]
    public async Task GetUsersInBoardQuery_ReturnsUsers()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var query = new GetUsersInBoardQuery(TestSeeder.DefaultUserId, boardId);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, u => u.UserId == TestSeeder.DefaultUserId);
    }
}