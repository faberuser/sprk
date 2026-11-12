using System;
using System.Collections.Generic;

namespace NVespa.NSocket;

public class SocketHandler<TContext> : IHandler
{
	private readonly Dictionary<string, ISocketService> _serviceDictionary;

	private TContext _context;

	public SocketHandler()
	{
		_serviceDictionary = new Dictionary<string, ISocketService>();
	}

	public void Clear()
	{
		_serviceDictionary.Clear();
	}

	public void Register(ISocketService service)
	{
		string name = service.GetHandleType().Name;
		_serviceDictionary.Remove(name);
		_serviceDictionary.Add(name, service);
	}

	public void Register<T>(Action<T> action) where T : class
	{
		DelegateSocketService<T> service = new DelegateSocketService<T>
		{
			Handler = action
		};
		Register(service);
	}

	public void SetContext(TContext context)
	{
		_context = context;
	}

	public void Handle(string type, string content)
	{
		ISocketService value = null;
		_serviceDictionary.TryGetValue(type, out value);
		value?.Handle(_context, content);
	}
}
