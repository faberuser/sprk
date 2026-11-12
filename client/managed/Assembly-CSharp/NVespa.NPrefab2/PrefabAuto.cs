using UnityEngine;

namespace NVespa.NPrefab2;

public class PrefabAuto<T> : IPrefabAuto where T : Component
{
	private Prefab<T> _value;

	public string PrefabPath { get; private set; }

	public bool IsGlobal { get; private set; }

	public bool IsPooling { get; private set; }

	public bool TouchMaterial { get; private set; }

	public EPrefabType PrefabType { get; private set; }

	public Prefab<T> Value
	{
		get
		{
			ReservePrefab();
			return _value;
		}
	}

	public Factory<T> Factory
	{
		get
		{
			if (Value != null)
			{
				return Value.Factory;
			}
			return null;
		}
	}

	bool IPrefabAuto.HasPrefab => _value != null;

	bool IPrefabAuto.IsOpen
	{
		get
		{
			if (_value != null)
			{
				return !_value.IsClosed;
			}
			return false;
		}
	}

	public PrefabAuto(string path)
		: this(path, isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: false)
	{
	}

	public PrefabAuto(string path, bool isGlobal, EPrefabType prefabType)
		: this(path, isGlobal, prefabType, pooling: true, touchMaterial: false)
	{
	}

	public PrefabAuto(string path, bool isGlobal, EPrefabType prefabType, bool pooling, bool touchMaterial)
	{
		PrefabPath = path;
		IsGlobal = isGlobal;
		IsPooling = pooling;
		TouchMaterial = touchMaterial;
		PrefabType = prefabType;
	}

	public void OnSceneChanged()
	{
		ReleasePrefab();
	}

	public void ReleasePrefab()
	{
		if (_value != null)
		{
			_value = null;
		}
	}

	public void ReservePrefab()
	{
		if (_value == null || _value.IsClosed)
		{
			_value = SingletonAuto<Container>.instance.Load<T>(PrefabPath, IsGlobal, PrefabType, IsPooling, TouchMaterial);
			SingletonAuto<PrefabAutoContainer>.instance.Add(this);
		}
	}

	public AsyncLoader.IPrefabLoad ReserveAsync()
	{
		return SingletonUpdater<AsyncLoader>.instance.LoadComponent<T>(PrefabPath, IsGlobal, PrefabType, IsPooling, TouchMaterial);
	}
}
