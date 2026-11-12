using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NDataAsset;

public abstract class IndexableDataAsset<TKey, TData> : ScriptableObject
{
	public TData[] Data;

	public Dictionary<TKey, List<TData>> Dict = new Dictionary<TKey, List<TData>>();

	private static Dictionary<Type, IndexableDataAsset<TKey, TData>> _singletons;

	protected virtual bool AlwaysReloadAsset => false;

	protected abstract TKey GetKey(TData t);

	public void BuildFrom(List<object> data)
	{
		Data = new TData[data.Count];
		for (int i = 0; i < data.Count; i++)
		{
			Data[i] = (TData)data[i];
		}
	}

	public static TAsset GetInstance<TAsset>() where TAsset : IndexableDataAsset<TKey, TData>
	{
		if (_singletons == null)
		{
			_singletons = new Dictionary<Type, IndexableDataAsset<TKey, TData>>();
		}
		Type typeFromHandle = typeof(TAsset);
		if (_singletons.TryGetValue(typeFromHandle, out var value))
		{
			return value as TAsset;
		}
		value = Resources.Load("DataAsset/" + typeFromHandle.Name) as TAsset;
		value.BuildIndex();
		if (!value.AlwaysReloadAsset)
		{
			_singletons.Add(typeFromHandle, value);
		}
		return value as TAsset;
	}

	public void BuildIndex()
	{
		Dict.Clear();
		TData[] data = Data;
		foreach (TData val in data)
		{
			TKey key = GetKey(val);
			if (!Dict.TryGetValue(key, out var value))
			{
				value = new List<TData>();
				Dict[key] = value;
			}
			value.Add(val);
		}
	}

	public List<TData> Get(TKey key)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Dict, key, null);
	}
}
