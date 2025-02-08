using System.Reflection;
using InboxOutboxPatternDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace InboxOutboxPatternDemo.Contexts;

public class AppDbContext : DbContext
{
    public const string schema = "dbo";

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderOutbox> OrderOutboxes { get; set; }
    public DbSet<OrderInbox> OrderInboxes { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.HasDefaultSchema(schema: schema);
    }
}