namespace NGame2.NMessage.NFastHandler.NEtc;

public class HandlerWidgetMessage : FastHandler<WidgetMessage>
{
	protected override bool OnMessage(WidgetMessage message)
	{
		return true;
	}
}
