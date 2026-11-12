using System.Collections.Generic;
using NGameAsset;
using NVespa.NPrefab2.NAsyncLoad;
using UnityEngine;

namespace NVespa.NPrefab2;

public class AsyncLoader : SingletonUpdater<AsyncLoader>
{
	public interface IPrefabLoad : IAsyncLoadBase
	{
		PrefabKey Key { get; }

		float BeginLoad { get; }

		float EndLoad { get; }

		void StartLoad();
	}

	public class AlreadyLoaded : IPrefabLoad, IAsyncLoadBase
	{
		public PrefabKey Key { get; private set; }

		public float BeginLoad { get; private set; }

		public float EndLoad { get; private set; }

		public string KeyDesc => Key.Path;

		public bool IsDone => true;

		public bool IsValid => true;

		public string LoadMessage => string.Empty;

		public AlreadyLoaded(PrefabKey key)
		{
			Key = key;
			BeginLoad = Time.realtimeSinceStartup;
			EndLoad = Time.realtimeSinceStartup;
		}

		public void StartLoad()
		{
		}
	}

	public interface IReserveComponent
	{
		void Tick();

		bool IsEnd();

		string Dump();
	}

	public class ReserveComponent<T> : IReserveComponent where T : Component
	{
		public PrefabAuto<T> PrefabAuto;

		public int Current;

		public int Count;

		void IReserveComponent.Tick()
		{
			Current++;
			if (PrefabAuto.Factory != null)
			{
				PrefabAuto.Factory.ReserveComponent(Current);
			}
		}

		bool IReserveComponent.IsEnd()
		{
			return Current >= Count;
		}

		string IReserveComponent.Dump()
		{
			return $"{PrefabAuto.PrefabPath}({Current}/{Count})";
		}
	}

	private Dictionary<PrefabKey, IPrefabLoad> _componentDic = new Dictionary<PrefabKey, IPrefabLoad>();

	private Queue<IPrefabLoad> _pendingQueue = new Queue<IPrefabLoad>();

	private List<IPrefabLoad> _loadingList = new List<IPrefabLoad>();

	private int _currentReserveIndex;

	private List<IReserveComponent> _reserveComponentList = new List<IReserveComponent>();

	public void PendingReserveComponent<T>(PrefabAuto<T> prefabAuto, int count) where T : Component
	{
		if (prefabAuto.Factory != null)
		{
			ReserveComponent<T> reserveComponent = new ReserveComponent<T>();
			reserveComponent.PrefabAuto = prefabAuto;
			reserveComponent.Count = count;
			_reserveComponentList.Add(reserveComponent);
		}
	}

	public IPrefabLoad LoadComponent<T>(string path, bool isGlobal, EPrefabType prefabType, bool isPooling, bool touchMaterial) where T : Component
	{
		PrefabKey prefabKey = new PrefabKey
		{
			IsGlobal = isGlobal,
			IsPooling = isPooling,
			TouchMaterial = touchMaterial,
			PrefabType = prefabType,
			Type = typeof(T),
			Path = path
		};
		if (SingletonAuto<Container>.instance.Contains<T>(prefabKey))
		{
			return new AlreadyLoaded(prefabKey);
		}
		IPrefabLoad vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_componentDic, prefabKey, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		ComponentLoad<T> componentLoad = new ComponentLoad<T>();
		componentLoad.Open(prefabKey);
		_componentDic.Add(prefabKey, componentLoad);
		_pendingQueue.Enqueue(componentLoad);
		return componentLoad;
	}

	public override void ChainUpdate()
	{
		for (int i = 0; i < _loadingList.Count; i++)
		{
			if (_loadingList[i].IsDone)
			{
				_componentDic.Remove(_loadingList[i].Key);
				_loadingList.RemoveAt(i);
				i--;
			}
		}
		while (_loadingList.Count < 10 && _pendingQueue.Count > 0)
		{
			IPrefabLoad prefabLoad = _pendingQueue.Dequeue();
			prefabLoad.StartLoad();
			_loadingList.Add(prefabLoad);
		}
		if (_reserveComponentList.Count > 0)
		{
			if (_currentReserveIndex >= _reserveComponentList.Count)
			{
				_currentReserveIndex = 0;
			}
			IReserveComponent reserveComponent = _reserveComponentList[_currentReserveIndex];
			reserveComponent.Tick();
			if (reserveComponent.IsEnd())
			{
				_reserveComponentList.RemoveAt(_currentReserveIndex);
			}
			else
			{
				_currentReserveIndex++;
			}
		}
	}

	public void DestroyOnSceneChanged()
	{
		for (int i = 0; i < _loadingList.Count; i++)
		{
			if (!_loadingList[i].Key.IsGlobal)
			{
				_componentDic.Remove(_loadingList[i].Key);
				_loadingList.RemoveAt(i);
				i--;
			}
		}
		List<IPrefabLoad> list = new List<IPrefabLoad>(_pendingQueue);
		_pendingQueue.Clear();
		for (int j = 0; j < list.Count; j++)
		{
			IPrefabLoad prefabLoad = list[j];
			if (!prefabLoad.Key.IsGlobal)
			{
				_componentDic.Remove(_loadingList[j].Key);
				list.RemoveAt(j);
				j--;
			}
			else
			{
				_pendingQueue.Enqueue(prefabLoad);
			}
		}
		_reserveComponentList.Clear();
		_currentReserveIndex = 0;
	}
}
