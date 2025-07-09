using Messages.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateDefaultBuilder(args);

builder.UseWolverine(opts =>
{
    opts.UseRabbitMq("amqp://user:password@localhost:5672/wolves")
        .AutoProvision();
    opts.Publish(c =>
    {
        c.MessagesFromNamespaceContaining<InvitationAccepted>();
        c.ToRabbitExchange("my-exchange");
    });
});

var app = builder.Build();

await app.StartAsync();

var messaging = app.Services.GetRequiredService<IMessageBus>();
await messaging.PublishAsync(new InvitationIssued
{
    Id = Guid.CreateVersion7().ToString()
});

await app.StopAsync();  