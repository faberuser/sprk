using System;
using System.Collections.Generic;
using NVespa;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ProxyContainer
{
	public ItemIconComponent Owner;

	private Dictionary<Type, ProxyBase> _dic = new Dictionary<Type, ProxyBase>();

	public void OnRelease()
	{
		foreach (KeyValuePair<Type, ProxyBase> item in _dic)
		{
			item.Value.Release();
			item.Value.DestroyThis();
		}
		_dic.Clear();
	}

	public T Get<T>() where T : GenericProxyBase<T>, new()
	{
		if (_dic.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		T val = SingletonAuto<ReservePool<T>>.instance.Create();
		val.Owner = Owner;
		_dic.Add(typeof(T), val);
		return val;
	}

	public bool IsActive<T>() where T : ProxyBase
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, typeof(T), null)?.IsActive() ?? false;
	}

	public void ReleaseProxy<T>() where T : GenericProxyBase<T>, new()
	{
		ProxyBase vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, typeof(T), null);
		if (vALUE != null)
		{
			vALUE.Release();
			vALUE.DestroyThis();
			_dic.Remove(typeof(T));
		}
	}

	public void OnDisable()
	{
		foreach (KeyValuePair<Type, ProxyBase> item in _dic)
		{
			item.Value.OnDisable();
		}
	}

	public void ChainUpdate()
	{
		foreach (KeyValuePair<Type, ProxyBase> item in _dic)
		{
			item.Value.ChainUpdate();
		}
	}
}
