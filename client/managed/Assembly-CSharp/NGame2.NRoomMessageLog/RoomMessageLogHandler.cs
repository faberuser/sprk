using NGame2.NMessageLog;
using NShared.NRoomMessage;
using NVespa;

namespace NGame2.NRoomMessageLog;

public class RoomMessageLogHandler : SingletonAuto<RoomMessageLogHandler>
{
	private readonly MessageProtocolHandlerContainer<RoomMessageNot> _handlerContainer;

	public RoomMessageLogHandler()
	{
		_handlerContainer = new MessageProtocolHandlerContainer<RoomMessageNot>("ROOM");
	}

	public void RegisterHandler(IMessageProtocolHandler handler)
	{
		_handlerContainer.RegisterHandler(handler);
	}

	public void Handle(RoomMessageNot messageNot)
	{
		_handlerContainer.Handle(messageNot);
	}
}
