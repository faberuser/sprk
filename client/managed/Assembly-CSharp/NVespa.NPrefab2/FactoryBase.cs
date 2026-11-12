using System.Collections.Generic;
using NVespa.NPrefab2.NEvent;
using UnityEngine;

namespace NVespa.NPrefab2;

public abstract class FactoryBase
{
	public static LocalStorageBool ReusePrefab = new LocalStorageBool(i_default: true, "Vespa_Prefab_ReusePrefab");

	protected GameObject _loadGo;

	protected bool _isGlobal;

	protected bool _isPooling;

	protected bool _touchMaterial;

	protected Transform _unusedRoot;

	public static void ReleaseComponent(Component comp)
	{
		if (comp == null)
		{
			return;
		}
		LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(comp.transform);
		if (linkFactoryComponent == null)
		{
			VespaDebug.LogError("Release Invalid Item: " + comp.gameObject.name);
			comp.gameObject.SetActive(value: false);
			comp.transform.parent = null;
			return;
		}
		linkFactoryComponent.Lock.Decrease(null);
		IReleaseEvent[] components = comp.gameObject.GetComponents<IReleaseEvent>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(components); i++)
		{
			components[i].OnRelease();
		}
		linkFactoryComponent.Link.ReleaseInternal(comp);
	}

	public static void ReleaseGameObject(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(go.transform);
		if (linkFactoryComponent == null)
		{
			VespaDebug.LogError("Release Invalid Item: " + go.name);
			go.SetActive(value: false);
			go.transform.parent = null;
			return;
		}
		linkFactoryComponent.Lock.Decrease(null);
		IReleaseEvent[] components = go.GetComponents<IReleaseEvent>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(components); i++)
		{
			components[i].OnRelease();
		}
		linkFactoryComponent.Link.ReleaseInternal(go);
	}

	public static void SafeReleaseComponent<T>(ref T comp) where T : Component
	{
		ReleaseComponent(comp);
		comp = null;
	}

	public static void ReleaseAndClearList<T>(ref PoolingList<T> compList) where T : Component
	{
		for (int i = 0; i < compList.Count; i++)
		{
			ReleaseComponent(compList[i]);
		}
		compList.Clear();
	}

	public static void ReleaseAndClearList<T>(List<T> compList) where T : Component
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(compList))
		{
			for (int i = 0; i < compList.Count; i++)
			{
				ReleaseComponent(compList[i]);
			}
			compList.Clear();
		}
	}

	public static void ReleaseAndClearList<K, T>(ref PoolingDic<K, T> compDic) where T : Component
	{
		if (compDic.IsNullOrEmpty())
		{
			return;
		}
		foreach (T value in compDic.Values)
		{
			ReleaseComponent(value);
		}
		compDic.Clear();
	}

	public static void ReleaseAndClearList<K, T>(Dictionary<K, T> compList) where T : Component
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(compList))
		{
			return;
		}
		foreach (T value in compList.Values)
		{
			ReleaseComponent(value);
		}
		compList.Clear();
	}

	protected FactoryBase(GameObject loadGo, bool isGlobal, bool isPooling, bool touchMaterial)
	{
		_loadGo = loadGo;
		_isGlobal = isGlobal;
		_isPooling = isPooling;
		_touchMaterial = touchMaterial;
	}

	internal abstract void ReleaseInternal(Component comp);

	internal abstract void ReleaseInternal(GameObject go);
}
