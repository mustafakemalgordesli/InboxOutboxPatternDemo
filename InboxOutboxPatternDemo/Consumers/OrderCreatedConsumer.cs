using System.Text.Json;
using InboxOutboxPatternDemo.Contexts;
using InboxOutboxPatternDemo.Events;
using InboxOutboxPatternDemo.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InboxOutboxPatternDemo.Consumers;

public class OrderCreatedConsumer(AppDbContext dbContext) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        var result = await dbContext.OrderInboxes.AnyAsync(x => x.IdempotentToken == message.IdempotentToken);

        if (!result)
        {
            await dbContext.OrderInboxes.AddAsync(new OrderInbox
            {
                IdempotentToken = message.IdempotentToken,
                EventType = nameof(OrderCreatedEvent),
                EventPayload = JsonSerializer.Serialize(message),
                EventDate = DateTime.UtcNow,
            });
            await dbContext.SaveChangesAsync();
        }

        var orderInbox = await dbContext.OrderInboxes.FirstOrDefaultAsync(x => x.IdempotentToken == message.IdempotentToken && x.IsProcessed == false);

        if (orderInbox == null) return;

        try
        {
            OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.EventPayload);

            if (orderCreatedEvent == null) return;

            Console.WriteLine("{0} - {1}", orderCreatedEvent.CustomerName, orderCreatedEvent.ProductName);

            orderInbox.IsProcessed = true;
            orderInbox.ProcessedDate = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            orderInbox.Error = ex.Message;
        }

        await dbContext.SaveChangesAsync();
    }
}