using System;
using NVespa;

namespace NGame2;

public static class MessageUtil
{
	public static void SendDummyMessage<TArg, TMessage>(Action<TArg, TMessage> onMessage) where TArg : class where TMessage : SmallPooling<TMessage>, ISmallPoolingItem, IMessage, new()
	{
		TMessage val = SmallPooling<TMessage>.CreateInstance();
		val.IsDummy = true;
		Vespa_System_Action_Extension_Method.fire(onMessage, null, val);
	}

	public static void SendDummyMessage<T>(Action<T> onMessage) where T : SmallPooling<T>, ISmallPoolingItem, IMessage, new()
	{
		T val = SmallPooling<T>.CreateInstance();
		val.IsDummy = true;
		Vespa_System_Action_Extension_Method.fire(onMessage, val);
	}
}
