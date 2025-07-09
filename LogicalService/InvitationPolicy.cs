using Messages.Events;
using Wolverine;

namespace LogicalService;

public class InvitationPolicy : Saga
{
    public string Id { get; set; } = null!;

    public async ValueTask Start(InvitationIssued message, IMessageBus bus)
    {
        Id = message.Id;
        await bus.PublishAsync(new InvitationTimeout(message.Id), new DeliveryOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddSeconds(10)
        });
    }

    public static ValueTask<InvitationExpired> Handle(InvitationTimeout message, ILogger<InvitationPolicy> logger)
    {
        logger.LogInformation("Invitation with ID {Id} has timed out", message.Id);
        return ValueTask.FromResult(new InvitationExpired(message.Id));
    }
    
    // When we use this method instead of the static above, it doesnt trigger the NotFound method at the same time
    // public InvitationExpired Handle(InvitationTimeout message, ILogger<InvitationPolicy> logger)
    // {
    //     logger.LogInformation("Invitation with ID {Id} has timed out", message.Id);
    //     return new InvitationExpired(message.Id);
    // }
    
    public void Handle(InvitationExpired message, ILogger<InvitationPolicy> logger)
    {
        logger.LogInformation("Completing saga with id {ID}", message.Id);
        MarkCompleted();
    }
    
    public void Handle(InvitationAccepted message, ILogger<InvitationPolicy> logger)
    {
        logger.LogInformation("Invitation has been accepted. Deleting saga with id {Id}", message.Id);
        MarkCompleted();
    }

    public static void NotFound(InvitationTimeout timeout, ILogger<InvitationPolicy> logger)
    {
        logger.LogError("Saga with {Id} has been completed already", timeout.Id);
    }
}