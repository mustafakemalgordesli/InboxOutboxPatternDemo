using System.Text.Json;
using InboxOutboxPatternDemo.Contexts;
using InboxOutboxPatternDemo.Events;
using InboxOutboxPatternDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace InboxOutboxPatternDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(AppDbContext appDbContext) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            using var transaction = await appDbContext.Database.BeginTransactionAsync();
            try
            {
                await appDbContext.Orders.AddAsync(order);
                var result = await appDbContext.SaveChangesAsync();

                if (result <= 0) throw new Exception("Failed to save customer");

                var customerCreatedEvent = new OrderCreatedEvent
                {
                    IdempotentToken = Guid.NewGuid(),
                    CustomerName = order.CustomerName,
                    ProductName = order.ProductName
                };

                var outboxMessage = new OrderOutbox
                {
                    IdempotentToken = customerCreatedEvent.IdempotentToken,
                    EventPayload = JsonSerializer.Serialize(customerCreatedEvent),
                    EventType = nameof(OrderCreatedEvent),
                    EventDate = DateTime.UtcNow,
                };

                await appDbContext.OrderOutboxes.AddAsync(outboxMessage);
                result = await appDbContext.SaveChangesAsync();

                if (result <= 0) throw new Exception("Failed to save outbox");

                await transaction.CommitAsync();

                return Created($"/api/orders/{order.Id}", order);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
