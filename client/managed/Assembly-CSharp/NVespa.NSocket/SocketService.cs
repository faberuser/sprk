using System;
using UnityEngine;

namespace NVespa.NSocket;

public abstract class SocketService<TContext, T> : ISocketService where TContext : class
{
	Type ISocketService.GetHandleType()
	{
		return typeof(T);
	}

	void ISocketService.Handle(object context, string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			T val = JsonMarshaler.Parse<T>(content);
			if (val == null)
			{
				Debug.LogError("Parse response error : " + typeof(T).Name + " data=" + content);
			}
			else
			{
				Handle(context as TContext, val);
			}
		}
	}

	public abstract void Handle(TContext context, T data);
}
