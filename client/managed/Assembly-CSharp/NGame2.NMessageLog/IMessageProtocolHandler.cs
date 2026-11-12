using System;

namespace NGame2.NMessageLog;

public interface IMessageProtocolHandler
{
	Type GetProtocolType();

	void Run(long senderId, string protocolString);
}
