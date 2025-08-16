using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskFlow.Application.Configuration;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class EmailService(IOptions<EmailOptions> emailOptions) : IEmailService
{
    private readonly string _eoSmtpHost = emailOptions.Value.SmtpHost ?? throw new ArgumentNullException("EmailOptions:SmtpHost");
    private readonly int _eoSmtpPort = emailOptions.Value.SmtpPort ?? throw new ArgumentNullException("EmailOptions:SmtpPort");
    private readonly string _eoSmtpUser = emailOptions.Value.SmtpUser ?? throw new ArgumentNullException("EmailOptions:SmtpUser");
    private readonly string _eoSmtpPass = emailOptions.Value.SmtpPass ?? throw new ArgumentNullException("EmailOptions:SmtpPass");
    private readonly string _eoFrom = emailOptions.Value.From ?? throw new ArgumentNullException("EmailOptions:From");
    private readonly bool _eoEnableSsl = emailOptions.Value.EnableSsl ?? throw new ArgumentNullException("EmailOptions:EnableSsl");
        
    public async Task SendAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_eoFrom));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_eoSmtpHost,_eoSmtpPort,_eoEnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
        await smtp.AuthenticateAsync(_eoSmtpUser , _eoSmtpPass );
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}