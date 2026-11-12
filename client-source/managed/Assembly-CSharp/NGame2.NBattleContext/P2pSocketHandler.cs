using System;
using System.Collections.Generic;
using NGame2.NMessageLog;
using NVespa;
using NVespa.NSocket;

namespace NGame2.NBattleContext;

public class P2pSocketHandler : IHandler
{
	private List<IMessageProtocolHandler> _protocolHandlerList = new List<IMessageProtocolHandler>();

	void IHandler.Register<T>(Action<T> handler)
	{
		Action<long, T> handler2 = delegate(long senderId, T data)
		{
			handler(data);
		};
		DelegateMessageProtocolHandler<T> delegateMessageProtocolHandler = new DelegateMessageProtocolHandler<T>
		{
			Handler = handler2
		};
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(delegateMessageProtocolHandler);
		_protocolHandlerList.Add(delegateMessageProtocolHandler);
	}

	void IHandler.Clear()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_protocolHandlerList); i++)
		{
			IMessageProtocolHandler handler = _protocolHandlerList[i];
			SingletonAuto<MessageLogHandler>.instance.RemoveHandler(handler);
		}
		_protocolHandlerList.Clear();
	}
}
