namespace InboxOutboxPatternDemo.Events;

public class OrderCreatedEvent
{
    public string ProductName { get; set; }
    public string CustomerName { get; set; }
    public Guid IdempotentToken { get; set; }
}
