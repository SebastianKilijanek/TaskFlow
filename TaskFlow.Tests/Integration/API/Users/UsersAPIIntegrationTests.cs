using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Integration.API.Users;

[Collection("SequentialTests")]
public class UsersApiIntegrationTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task GetUsers_WithAdminRole_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims(new Claim(ClaimTypes.Role, "Admin"));

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
        Assert.NotNull(users);
        Assert.Contains(users, u => u.UserName == "Tester");
    }

    [Fact]
    public async Task GetUsers_WithoutAdminRole_Returns403()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithExistingIdAndAdminRole_Returns200()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims(new Claim(ClaimTypes.Role, "Admin"));

        // Act
        var response = await client.GetAsync($"/api/users/{user.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var resultUser = await response.Content.ReadFromJsonAsync<UserDTO>();
        Assert.NotNull(resultUser);
        Assert.Equal(user.Id, resultUser.Id);
    }

    [Fact]
    public async Task GetUser_WithNonExistingId_Returns404()
    {
        // Arrange
        factory.GetTestScope();
        var client = factory.CreateClientWithClaims(new Claim(ClaimTypes.Role, "Admin"));
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithAdminRole_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims(new Claim(ClaimTypes.Role, "Admin"));
        var command = new UpdateUserCommand(user.Id, "newtest@email.com", "NewTester", "Admin");

        testScope.DbContext.ChangeTracker.Clear();
        
        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}", command);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updatedUser = await testScope.UnitOfWork.Repository<User>().GetByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("NewTester", updatedUser.UserName);
        Assert.Equal("newtest@email.com", updatedUser.Email);
        Assert.Equal(UserRole.Admin, updatedUser.Role);
    }

    [Fact]
    public async Task DeleteUser_WithAdminRole_Returns204()
    {
        // Arrange
        var testScope = factory.GetTestScope();
        var user = await TestSeeder.SeedUser(testScope);
        var client = factory.CreateClientWithClaims(new Claim(ClaimTypes.Role, "Admin"));
        
        testScope.DbContext.ChangeTracker.Clear();

        // Act
        var response = await client.DeleteAsync($"/api/users/{user.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedUser = await testScope.UnitOfWork.Repository<User>().GetByIdAsync(user.Id);
        Assert.Null(deletedUser);
    }
}