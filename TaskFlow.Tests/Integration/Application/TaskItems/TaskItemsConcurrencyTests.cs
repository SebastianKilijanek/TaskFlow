using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Application.TaskItems;

[Collection("SequentialTests")]
public class TaskItemsConcurrencyTests(TestWebApplicationFactory<TaskFlow.API.AssemblyReference> factory)
    : IClassFixture<TestWebApplicationFactory<TaskFlow.API.AssemblyReference>>
{
    [Fact]
    public async Task ConcurrentMoves_SecondSave_ThrowsConcurrencyException()
    {
        var seedScope = factory.GetTestScope();
        await TestSeeder.SeedUser(seedScope);
        var boardId = await TestSeeder.SeedBoard(seedScope);
        var column1 = await TestSeeder.SeedColumn(seedScope, boardId, "To Do");
        var column2 = await TestSeeder.SeedColumn(seedScope, boardId, "Done");
        var taskId = await TestSeeder.SeedTask(seedScope, column1);

        using var scope1 = factory.Services.CreateScope();
        using var scope2 = factory.Services.CreateScope();
        var uow1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var uow2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var task1 = await uow1.Repository<TaskItem>().GetByIdAsync(taskId);
        var task2 = await uow2.Repository<TaskItem>().GetByIdAsync(taskId);

        task1!.ColumnId = column2;
        uow1.Repository<TaskItem>().Update(task1);
        await uow1.SaveChangesAsync();

        task2!.Position = 99;
        uow2.Repository<TaskItem>().Update(task2);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => uow2.SaveChangesAsync());
    }

    [Fact]
    public async Task SequentialMoves_SecondSave_Succeeds()
    {
        var seedScope = factory.GetTestScope();
        await TestSeeder.SeedUser(seedScope);
        var boardId = await TestSeeder.SeedBoard(seedScope);
        var column1 = await TestSeeder.SeedColumn(seedScope, boardId, "To Do");
        var column2 = await TestSeeder.SeedColumn(seedScope, boardId, "Done");
        var taskId = await TestSeeder.SeedTask(seedScope, column1);

        using var scope1 = factory.Services.CreateScope();
        using var scope2 = factory.Services.CreateScope();
        var uow1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var uow2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var task1 = await uow1.Repository<TaskItem>().GetByIdAsync(taskId);
        task1!.ColumnId = column2;
        uow1.Repository<TaskItem>().Update(task1);
        await uow1.SaveChangesAsync();

        var task2 = await uow2.Repository<TaskItem>().GetByIdAsync(taskId);
        task2!.Position = 99;
        uow2.Repository<TaskItem>().Update(task2);
        await uow2.SaveChangesAsync();

        var task = await uow2.Repository<TaskItem>().GetByIdAsync(taskId);
        Assert.Equal(99, task!.Position);
    }
}