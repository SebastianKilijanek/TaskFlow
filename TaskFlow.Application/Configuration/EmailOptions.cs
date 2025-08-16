namespace TaskFlow.Application.Configuration;

public class EmailOptions
{
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
    public string? From { get; set; }
    public bool? EnableSsl { get; set; }
}
