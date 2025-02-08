using System.ComponentModel.DataAnnotations;

namespace InboxOutboxPatternDemo.Models;

public class OrderOutbox
{
    [Key]
    public Guid IdempotentToken { get; set; }
    public string EventType { get; set; }
    public string EventPayload { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public bool IsSent { get; set; }
    public DateTime? SentDate { get; set; }
    public string? Error { get; set; }
}

