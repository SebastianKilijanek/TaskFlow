using TaskFlow.Application.Users.Commands;
using Xunit;
using TaskFlow.Application.Users.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Integration.Application.Users;

[Collection("SequentialTests")]
public class UsersIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task GetUsersQuery_ReturnsUsers()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var query = new GetUsersQuery();

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, u => u.UserName == user.UserName);
    }

    [Fact]
    public async Task GetUserByIdQuery_ReturnsExistingUser()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await testScope.Mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task UpdateUserCommand_UpdatesUserInDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var command = new UpdateUserCommand(user.Id, "othertest@test.com", "OtherTester", "Admin");
        
        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var updatedUser = await testScope.UnitOfWork.Repository<User>().GetByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("othertest@test.com", updatedUser.Email);
        Assert.Equal("OtherTester", updatedUser.UserName);
        Assert.Equal(UserRole.Admin, updatedUser.Role);
    }

    [Fact]
    public async Task DeleteUserCommand_RemovesUserFromDatabase()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var command = new DeleteUserCommand(user.Id);

        // Act
        await testScope.Mediator.Send(command);

        // Assert
        var deletedUser = await testScope.UnitOfWork.Repository<User>().GetByIdAsync(user.Id);
        Assert.Null(deletedUser);
    }
}