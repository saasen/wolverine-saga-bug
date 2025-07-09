using Wolverine.Persistence.Sagas;

namespace Messages.Events;

public class InvitationIssued
{
    [SagaIdentity] public string Id { get; set; }
};