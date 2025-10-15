using System.Net;
using System.Net.Http.Json;
using TaskFlow.Application.UserBoards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Integration.API.UserBoards;

[Collection("SequentialTests")]
public class UserBoardApiIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    private const string USER_BOARDS_BASE_URL = "/api/boards";

    [Fact]
    public async Task GetUsersInBoard_Should_ReturnUsersAndOk()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);

        // Act
        var response = await client.GetAsync($"{USER_BOARDS_BASE_URL}/{boardId}/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserBoardDTO>>();
        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Contains(users, u => u.UserId == TestSeeder.DefaultUserId);
    }

    [Fact]
    public async Task AddUserToBoard_Should_AddUserAndReturnNoContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var userToAdd = await TestSeeder.SeedUser(testScope, userId: Guid.NewGuid(), email: "othertest@test.com");
        var payload = new AddUserToBoardDTO { UserEmail = userToAdd.Email, BoardRole = BoardRole.Editor };

        // Act
        var response = await client.PostAsJsonAsync($"{USER_BOARDS_BASE_URL}/{boardId}/users", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var userBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(userToAdd.Id, boardId);
        Assert.NotNull(userBoard);
        Assert.Equal(BoardRole.Editor, userBoard.BoardRole);
    }

    [Fact]
    public async Task RemoveUserFromBoard_Should_RemoveUserAndReturnNoContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var userToRemove = await TestSeeder.SeedUser(testScope, userId: Guid.NewGuid(), email: "othertest@test.com");
        await TestSeeder.SeedUserBoard(testScope, userToRemove.Id, boardId, BoardRole.Viewer);
        
        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.DeleteAsync($"{USER_BOARDS_BASE_URL}/{boardId}/users/{userToRemove.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(userToRemove.Id, boardId);
        Assert.Null(deletedUserBoard);
    }

    [Fact]
    public async Task ChangeUserBoardRole_Should_ChangeRoleAndReturnNoContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var userToChange = await TestSeeder.SeedUser(testScope, userId: Guid.NewGuid(), email: "othertest@test.com");
        await TestSeeder.SeedUserBoard(testScope, userToChange.Id, boardId, BoardRole.Viewer);
        var payload = new ChangeUserBoardRoleDTO { NewRole = BoardRole.Editor };
        
        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.PutAsJsonAsync($"{USER_BOARDS_BASE_URL}/{boardId}/users/{userToChange.Id}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(userToChange.Id, boardId);
        Assert.NotNull(updatedUserBoard);
        Assert.Equal(BoardRole.Editor, updatedUserBoard.BoardRole);
    }

    [Fact]
    public async Task TransferBoardOwnership_Should_TransferOwnershipAndReturnNoContent()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var client = factory.CreateClientWithClaims();
        var ownerId = await TestSeeder.SeedUser(testScope);
        var boardId = await TestSeeder.SeedBoard(testScope);
        var newOwner = await TestSeeder.SeedUser(testScope, userId: Guid.NewGuid(), email: "othertest@test.com");
        await TestSeeder.SeedUserBoard(testScope, newOwner.Id, boardId, BoardRole.Editor);
        
        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.PostAsync($"{USER_BOARDS_BASE_URL}/{boardId}/users/{newOwner.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var oldOwnerUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(ownerId.Id, boardId);
        var newOwnerUserBoard = await testScope.UnitOfWork.Repository<UserBoard>().GetByIdAsync(newOwner.Id, boardId);
        Assert.NotNull(oldOwnerUserBoard);
        Assert.NotNull(newOwnerUserBoard);
        Assert.Equal(BoardRole.Editor, oldOwnerUserBoard.BoardRole);
        Assert.Equal(BoardRole.Owner, newOwnerUserBoard.BoardRole);
    }
}