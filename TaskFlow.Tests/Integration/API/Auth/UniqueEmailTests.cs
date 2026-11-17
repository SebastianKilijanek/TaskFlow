using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.API.Auth;

[Collection("SequentialTests")]
public class UniqueEmailTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "dup@test.com", hashPassword: true);
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("dup@test.com", "Other", "Password123!");

        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmailDifferentCase_Returns409()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "dup@test.com", hashPassword: true);
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("DUP@Test.com", "Other", "Password123!");

        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_ConcurrentRequests_ReturnsOkAndConflict()
    {
        factory.GetTestScope();
        var client = factory.CreateClient();
        var command = new RegisterUserCommand("concurrent@test.com", "Other", "Password123!");

        var first = client.PostAsJsonAsync("/api/auth/register", command);
        var second = client.PostAsJsonAsync("/api/auth/register", command);
        var responses = await Task.WhenAll(first, second);

        var statuses = responses.Select(r => (int)r.StatusCode).OrderBy(x => x).ToArray();

        Assert.Equal(new[] { 200, 409 }, statuses);
    }

    [Fact]
    public async Task DbContext_UniqueIndex_RejectsDuplicate()
    {
        var scope = factory.GetTestScope();
        await TestSeeder.SeedUser(scope, email: "index@test.com", hashPassword: true);

        var duplicate = new User
        {
            Id = Guid.NewGuid(),
            Email = "index@test.com",
            UserName = "Duplicate",
            PasswordHash = "x",
            Role = UserRole.User,
        };
        await scope.UnitOfWork.Repository<User>().AddAsync(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.UnitOfWork.SaveChangesAsync());
    }
}