using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NVespa;

public static class ReservePoolUtil
{
	public static object LockObj = new object();

	private static List<IReservePool> _poolList = new List<IReservePool>();

	public static void AddPool(IReservePool pool)
	{
		lock (LockObj)
		{
			_poolList.Add(pool);
		}
	}

	public static T Create<T>() where T : new()
	{
		return SingletonAuto<ReservePool<T>>.instance.Create();
	}

	public static void Release<T>(T item) where T : new()
	{
		if (item != null)
		{
			SingletonAuto<ReservePool<T>>.instance.Release(item);
		}
	}

	public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
	{
		return SingletonAuto<ReservePool<Dictionary<TKey, TValue>>>.instance.Create();
	}

	public static void ReleaseDictionary<TKey, TValue>(Dictionary<TKey, TValue> collection)
	{
		if (collection != null)
		{
			collection.Clear();
			SingletonAuto<ReservePool<Dictionary<TKey, TValue>>>.instance.Release(collection);
		}
	}

	public static List<T> CreateList<T>()
	{
		return SingletonAuto<ReservePool<List<T>>>.instance.Create();
	}

	public static void ReleaseList<T>(List<T> list)
	{
		if (list != null)
		{
			list.Clear();
			SingletonAuto<ReservePool<List<T>>>.instance.Release(list);
		}
	}

	public static void ClearUnusedAll()
	{
		lock (LockObj)
		{
			for (int i = 0; i < _poolList.Count; i++)
			{
				_poolList[i].ClearUnused();
			}
		}
	}

	public static void ReserveAll(bool resetAll)
	{
		lock (LockObj)
		{
			for (int i = 0; i < _poolList.Count; i++)
			{
				_poolList[i].Reserve(resetAll);
			}
		}
	}

	public static void DrawGui()
	{
		StringBuilder stringBuilder = new StringBuilder();
		lock (LockObj)
		{
			for (int i = 0; i < _poolList.Count; i++)
			{
				_poolList[i].GetDescription(stringBuilder);
			}
		}
		GUILayout.Label(stringBuilder.ToString());
	}
}
