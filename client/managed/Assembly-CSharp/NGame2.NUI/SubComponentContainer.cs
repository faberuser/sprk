using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI;

public class SubComponentContainer<TOwner> where TOwner : Component
{
	private Dictionary<Type, BaseSubComponent<TOwner>> _dic = new Dictionary<Type, BaseSubComponent<TOwner>>();

	public T GetSubComponent<T>() where T : BaseSubComponent<TOwner>
	{
		Type typeFromHandle = typeof(T);
		BaseSubComponent<TOwner> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, typeFromHandle, null);
		if (vALUE == null)
		{
			return null;
		}
		return vALUE as T;
	}

	public void ForEach<T>(Action<BaseSubComponent<TOwner>> action) where T : BaseSubComponent<TOwner>
	{
		foreach (KeyValuePair<Type, BaseSubComponent<TOwner>> item in _dic)
		{
			Vespa_System_Action_Extension_Method.fire(action, item.Value);
		}
	}

	public void ReleaseSubComponent()
	{
		FactoryBase.ReleaseAndClearList(_dic);
		_dic.Clear();
	}

	public T CreateSubComponent<T>(Transform t) where T : BaseSubComponent<TOwner>
	{
		T val = SingletonAuto<ComponentFactory>.instance.Create<T>(t);
		if (val == null)
		{
			return null;
		}
		_dic.Add(typeof(T), val);
		return val;
	}
}
