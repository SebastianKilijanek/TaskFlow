using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Tests.Application.UserBoards;

[Collection("SequentialTests")]
public class OutboxTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task AddUserToBoard_WritesOutboxRow()
    {
        var scope = factory.GetTestScope();
        var owner = await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var targetId = Guid.NewGuid();
        await TestSeeder.SeedUser(scope, targetId, "target@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, ownerId: owner.Id, name: "Board");

        var command = new AddUserToBoardCommand(owner.Id, boardId, "target@test.com", BoardRole.Editor);
        await scope.Mediator.Send(command);

        var outbox = await scope.UnitOfWork.Repository<OutboxMessage>()
            .ListReadOnlyAsync(m => m.Type == "email");
        var message = Assert.Single(outbox);

        Assert.Equal(OutboxStatus.Pending, message.Status);
        Assert.Contains("target@test.com", message.PayloadJson);
    }

    [Fact]
    public async Task AddUserToBoard_WhenValidationFails_NoOutboxRow()
    {
        var scope = factory.GetTestScope();
        var owner = await TestSeeder.SeedUser(scope, email: "owner@test.com");
        var boardId = await TestSeeder.SeedBoard(scope, ownerId: owner.Id, name: "Board");

        var command = new AddUserToBoardCommand(owner.Id, boardId, "not-an-email", BoardRole.Editor);
        await Assert.ThrowsAsync<BadRequestException>(() => scope.Mediator.Send(command));

        var outbox = await scope.UnitOfWork.Repository<OutboxMessage>()
            .ListReadOnlyAsync(m => m.Type == "email");
        Assert.Empty(outbox);
    }
}