using System;
using System.Collections.Generic;
using NShared.NJit.NGenerator;

namespace NShared.NJit.NResolver;

public static class ResolverContainer
{
	private static Dictionary<Type, IDataResolver> _dic;

	private static Dictionary<Type, IKeyComparer> _comparerDic;

	static ResolverContainer()
	{
		_dic = new Dictionary<Type, IDataResolver>();
		_comparerDic = new Dictionary<Type, IKeyComparer>();
		Reset();
	}

	public static void Reset()
	{
		_dic.Clear();
		_comparerDic.Clear();
		GenCodeContainer.Register();
	}

	public static void Add(IDataResolver resolver)
	{
		if (resolver != null && !_dic.ContainsKey(resolver.GetResolveType()))
		{
			_dic.Add(resolver.GetResolveType(), resolver);
		}
	}

	public static void AddComparer(IKeyComparer comparer)
	{
		if (comparer != null && !_comparerDic.ContainsKey(comparer.GetCompareType()))
		{
			_comparerDic.Add(comparer.GetCompareType(), comparer);
		}
	}

	public static IKeyComparerT<T> GetComparer<T>()
	{
		lock (_comparerDic)
		{
			return GetComparerInternal(typeof(T)) as IKeyComparerT<T>;
		}
	}

	private static IKeyComparer GetComparerInternal(Type type)
	{
		if (_comparerDic.TryGetValue(type, out var value))
		{
			return value;
		}
		value = UsingReflection.CreateComparer(type);
		if (value == null)
		{
			JitOption.OpenErrorPopup(type, isComparer: true);
			value = UsingReflection.CreateUnknownComparer(type);
		}
		if (!_comparerDic.ContainsKey(type))
		{
			_comparerDic.Add(type, value);
		}
		return value;
	}

	public static IDataResolverT<T> Get<T>()
	{
		return Get(typeof(T)) as IDataResolverT<T>;
	}

	public static IDataResolver Get(Type type)
	{
		lock (_dic)
		{
			return GetInternal(type);
		}
	}

	private static IDataResolver GetInternal(Type type)
	{
		if (_dic.TryGetValue(type, out var value))
		{
			return value;
		}
		if (JitOption.IsClient && !JitOption.IsUnityEditor)
		{
			return null;
		}
		value = UsingReflection.CreateResolver(type);
		if (value == null)
		{
			JitOption.OpenErrorPopup(type, isComparer: false);
			value = UsingReflection.CreateUnknownResolver(type);
		}
		if (!_dic.ContainsKey(type))
		{
			_dic.Add(type, value);
		}
		return value;
	}
}
