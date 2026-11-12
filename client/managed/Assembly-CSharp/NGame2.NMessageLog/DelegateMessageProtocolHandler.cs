using System;

namespace NGame2.NMessageLog;

public class DelegateMessageProtocolHandler<T> : MessageProtocolHandler<T>
{
	public Action<long, T> Handler;

	public override void Run(long senderId, T protocolData)
	{
		Vespa_System_Action_Extension_Method.fire(Handler, senderId, protocolData);
	}
}
