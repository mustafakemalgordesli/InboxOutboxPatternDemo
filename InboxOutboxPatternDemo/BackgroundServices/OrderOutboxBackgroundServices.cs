using System.Text.Json;
using InboxOutboxPatternDemo.Contexts;
using InboxOutboxPatternDemo.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InboxOutboxPatternDemo.BackgroundServices;

public class OutboxBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IPublishEndpoint _publishEndpoint = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IPublishEndpoint>();

        Console.WriteLine("Background Service Starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var _context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

            var outboxMessages = await _context.OrderOutboxes
                 .Where(x => !x.IsSent)
                 .OrderBy(x => x.EventDate)
                 .ToListAsync(stoppingToken);

            foreach (var outboxMessage in outboxMessages)
            {
                try
                {
                    OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(outboxMessage.EventPayload);

                    if (orderCreatedEvent != null)
                    {
                        Console.WriteLine("{0} Publishing message to the bus...", orderCreatedEvent.CustomerName);
                        await _publishEndpoint.Publish(orderCreatedEvent);
                        outboxMessage.IsSent = true;
                        outboxMessage.SentDate = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    outboxMessage.Error = ex.Message;
                }

                await _context.SaveChangesAsync(stoppingToken);
            }

            Task.Delay(10000, stoppingToken).Wait();
        }
    }
}
