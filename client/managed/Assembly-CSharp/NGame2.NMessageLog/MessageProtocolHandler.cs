using System;
using NVespa;

namespace NGame2.NMessageLog;

public abstract class MessageProtocolHandler<T> : IMessageProtocolHandler
{
	Type IMessageProtocolHandler.GetProtocolType()
	{
		return typeof(T);
	}

	void IMessageProtocolHandler.Run(long senderId, string protocolString)
	{
		T val = JsonMarshaler.Parse<T>(protocolString);
		if (val == null)
		{
			VespaDebug.LogError("Failed to parse message protocol : " + protocolString + " type=" + typeof(T).Name);
		}
		else
		{
			Run(senderId, val);
		}
	}

	public abstract void Run(long senderId, T protocolData);
}
