using System;

namespace NVespa.NSocket;

public class DelegateSocketService<T> : ISocketService where T : class
{
	public Action<T> Handler { get; set; }

	Type ISocketService.GetHandleType()
	{
		return typeof(T);
	}

	void ISocketService.Handle(object context, string content)
	{
		if (Handler != null)
		{
			T val = JsonMarshaler.Parse<T>(content);
			if (val != null)
			{
				Handler(val);
			}
		}
	}
}
