using System;
using System.Collections.Generic;

namespace NShared;

public class BattleEventRouter
{
	private readonly Dictionary<Type, Action<object, object>> _routerDictionary = new Dictionary<Type, Action<object, object>>();

	public void Register<TContext, T>(IBattleEventService<TContext, T> service) where TContext : IBattleEventServiceContext
	{
		_routerDictionary.Add(typeof(T), delegate(object b, object o)
		{
			service.Handle((TContext)b, (T)o);
		});
	}

	public void Handle<TContext, T>(TContext context, T data) where TContext : IBattleEventServiceContext
	{
		if (_routerDictionary.TryGetValue(typeof(T), out var value))
		{
			value?.Invoke(context, data);
		}
	}

	public bool HasHandler(Type type)
	{
		return _routerDictionary.ContainsKey(type);
	}

	public void UnitTest()
	{
		Type[] types = GetType().Assembly.GetTypes();
		Type typeFromHandle = typeof(BaseBattleCallback);
		Type[] array = types;
		foreach (Type type in array)
		{
			if (type != typeFromHandle && typeFromHandle.IsAssignableFrom(type))
			{
				HasHandler(type);
			}
		}
	}
}
