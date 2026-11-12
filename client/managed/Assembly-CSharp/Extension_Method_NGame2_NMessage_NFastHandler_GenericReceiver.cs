using NGame2;
using NGame2.NMessage.NFastHandler;

public static class Extension_Method_NGame2_NMessage_NFastHandler_GenericReceiver
{
	public static bool ISVALID<T>(this GenericReceiver<T> This) where T : IMessage
	{
		return This?.IsValid() ?? false;
	}
}
