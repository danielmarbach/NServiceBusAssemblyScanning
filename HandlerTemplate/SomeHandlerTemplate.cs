namespace NamespaceTemplate;

public class SomeHandlerTemplate : IHandleMessages<SomeMessageTemplate>
{
    public Task Handle(SomeMessageTemplate message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}