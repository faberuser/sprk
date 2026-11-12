using System.Collections;
using NGame2.NUI.NAtlasAssignComponent;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NPrefab2;

public class Factory<T> : FactoryBase where T : Component
{
	private int _createCount;

	private Queue _unusedQueue = new Queue();

	public bool IsClosed { get; private set; }

	public int UnusedCount => _unusedQueue.Count;

	public Factory(GameObject loadGo, bool isGlobal, bool isPooling, bool touchMaterial)
		: base(loadGo, isGlobal, isPooling, touchMaterial)
	{
		_loadGo.SetActive(value: false);
		InitUnusedRoot();
	}

	private void InitUnusedRoot()
	{
		if (_isPooling)
		{
			GameObject gameObject = ((!_isGlobal) ? SceneAutoManager.SingletonComponent : GlobalAutoManager.SingletonComponent);
			GameObject gameObject2 = new GameObject("Prefab2_Factory_" + Vespa_Unity_Object_Extension_Method.NAME(_loadGo));
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject2.transform, gameObject.transform);
			_unusedRoot = gameObject2.transform;
			gameObject2.SetActive(value: false);
		}
	}

	public void Close()
	{
		while (_unusedQueue.Count > 0)
		{
			T val = _unusedQueue.Dequeue() as T;
			if (!(val == null))
			{
				VespaObject.DestroyGo(val);
			}
		}
		if (_unusedRoot != null && _isGlobal)
		{
			VespaObject.DestroyGo(_unusedRoot);
			_unusedRoot = null;
		}
		_loadGo = null;
		IsClosed = true;
	}

	public void ReserveComponent()
	{
		ReserveComponent(1);
	}

	public void ReserveComponent(int count)
	{
		if (!_isPooling)
		{
			return;
		}
		int num = ((!_isGlobal) ? (count - _createCount) : (count - _unusedQueue.Count));
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				T unused = InstantiateNew(null, -1, setActive: false);
				PutUnused(unused);
			}
		}
	}

	public T Create(Transform parentTm, bool setActive = true)
	{
		return Create(parentTm, (parentTm == null) ? (-1) : parentTm.gameObject.layer, setActive);
	}

	public T Create(Transform parentTm, int layer, bool setActive = true)
	{
		T val = PopUnused(parentTm, layer, setActive);
		if (val == null)
		{
			val = InstantiateNew(parentTm, layer, setActive);
		}
		if (val == null)
		{
			return null;
		}
		if (val is InitOnCreate initOnCreate)
		{
			initOnCreate.OnCreate();
		}
		AtlasAssignerController component = val.GetComponent<AtlasAssignerController>();
		if (component != null)
		{
			component.Assign();
		}
		LinkFactoryComponent.IncreaseLock(val);
		return val;
	}

	internal override void ReleaseInternal(GameObject go)
	{
		T component = go.GetComponent<T>();
		if (component == null)
		{
			go.SetActive(value: false);
			go.transform.parent = null;
			VespaObject.DestroyGo(go);
		}
		else
		{
			ReleaseInternal(component);
		}
	}

	internal override void ReleaseInternal(Component comp)
	{
		if (comp is InitOnCreate initOnCreate)
		{
			initOnCreate.OnRelease();
		}
		if (IsClosed || !_isPooling)
		{
			comp.gameObject.SetActive(value: false);
			comp.transform.parent = null;
			VespaObject.DestroyGo(comp);
			return;
		}
		if (!(comp is T))
		{
			comp.gameObject.SetActive(value: false);
			comp.gameObject.transform.parent = null;
			VespaObject.DestroyGo(comp);
			return;
		}
		UIDragScrollView uIDragScrollView = SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.Get(comp.transform, isFast: true);
		if (uIDragScrollView != null)
		{
			uIDragScrollView.scrollView = null;
		}
		PutUnused(comp as T);
	}

	private void PutUnused(T unused)
	{
		_unusedQueue.Enqueue(unused);
		unused.gameObject.SetActive(value: false);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(unused.gameObject.transform, _unusedRoot);
	}

	private T PopUnused(Transform parentTm, int layer, bool setActive)
	{
		if (_unusedQueue.Count == 0)
		{
			return null;
		}
		while (_unusedQueue.Count > 0)
		{
			T val = _unusedQueue.Dequeue() as T;
			if (!(val == null))
			{
				if (parentTm != null)
				{
					AdjustLayer(val.gameObject, layer);
					Vespa_Unity_Transform_Extension_Method.ParentRelative(val.transform, parentTm);
				}
				val.gameObject.SetActive(setActive);
				return val;
			}
		}
		return null;
	}

	private T InstantiateNew(Transform parentTm, int layer, bool setActive)
	{
		GameObject gameObject = Object.Instantiate(_loadGo);
		if (gameObject == null)
		{
			return null;
		}
		if (parentTm != null)
		{
			Vespa_Unity_Transform_Extension_Method.ParentRelative(gameObject.transform, parentTm);
		}
		else
		{
			Vespa_Unity_Transform_Extension_Method.ParentRelative(gameObject.transform, _unusedRoot);
		}
		gameObject.SetActive(setActive);
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			VespaObject.DestroyGo(gameObject);
			return null;
		}
		_createCount++;
		AdjustLayer(gameObject, layer);
		SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.GetAuto(gameObject.transform).Link = this;
		return component;
	}

	private void AdjustLayer(GameObject targetGo, int layer)
	{
		if (layer >= 0 && targetGo.layer != layer)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(targetGo, layer);
		}
	}
}
