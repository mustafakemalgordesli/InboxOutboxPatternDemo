namespace InboxOutboxPatternDemo.Models;

public class Order
{
    public Guid Id { get; set; }
    public string ProductName { get; set; }
    public string CustomerName { get; set; }
}
