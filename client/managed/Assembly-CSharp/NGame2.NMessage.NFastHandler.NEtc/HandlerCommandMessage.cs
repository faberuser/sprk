namespace NGame2.NMessage.NFastHandler.NEtc;

public class HandlerCommandMessage : FastHandler<CommandMessage>
{
	protected override bool OnMessage(CommandMessage message)
	{
		return true;
	}
}
