using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Tests;

public class TestEmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string body)
    {
        return Task.CompletedTask;
    }
}