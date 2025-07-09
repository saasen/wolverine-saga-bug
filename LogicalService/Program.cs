using JasperFx;
using JasperFx.Resources;
using Messages.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;
var environment = builder.Environment;

builder.UseWolverine(c =>
{
    c.UseRabbitMq(config.GetConnectionString("RabbitMQ")!)
        .AutoProvision();

    c.Policies.MessageExecutionLogLevel(LogLevel.Information);
    c.Policies.MessageSuccessLogLevel(LogLevel.Information);
    
    c.PersistMessagesWithPostgresql(config.GetConnectionString("PostgreSQL")!);
    c.UseEntityFrameworkCoreTransactions();
    
    c.Policies.UseDurableInboxOnAllListeners();
    c.Policies.UseDurableOutboxOnAllSendingEndpoints();

    c.ListenToRabbitQueue("my-mh").ConfigureQueue(d =>
    {
        d.BindExchange("my-exchange");
    });
    
    c.Publish(rule =>
    {
        rule.MessagesFromAssemblyContaining<InvitationTimeout>();
        rule.ToRabbitExchange("my-exchange");
    });
    
    c.Services.AddResourceSetupOnStartup();
});

services.AddDbContext<MyDbContext>(x => x.UseNpgsql(config.GetConnectionString("PostgreSQL")!), ServiceLifetime.Singleton);

var app = builder.Build();
await app.RunJasperFxCommands(args);

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
}
