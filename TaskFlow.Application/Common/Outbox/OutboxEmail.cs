namespace TaskFlow.Application.Common.Outbox;

public record OutboxEmail(string To, string Subject, string Body);