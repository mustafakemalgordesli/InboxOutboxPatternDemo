using System.ComponentModel.DataAnnotations;

namespace InboxOutboxPatternDemo.Models;

public class OrderInbox
{
    [Key]
    public Guid IdempotentToken { get; set; }
    public string EventType { get; set; }
    public string EventPayload { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Error { get; set; }
}