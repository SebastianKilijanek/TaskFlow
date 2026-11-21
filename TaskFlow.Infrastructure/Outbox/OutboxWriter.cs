using System.Text.Json;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Outbox;

public class OutboxWriter(TaskFlowDbContext context) : IOutbox
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Add(string type, object payload)
    {
        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            Status = OutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        });
    }
}