namespace HandlerTemplate;

public class Handler1 : IHandleMessages<Message1>
{
    public Task Handle(Message1 message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}