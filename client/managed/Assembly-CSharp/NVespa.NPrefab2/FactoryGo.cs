using System.Collections;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NPrefab2;

public class FactoryGo : FactoryBase
{
	private Queue _unusedQueue = new Queue();

	public bool IsClosed { get; private set; }

	public int UnusedCount => _unusedQueue.Count;

	public FactoryGo(GameObject loadGo, bool isGlobal, bool isPooling, bool touchMaterial)
		: base(loadGo, isGlobal, isPooling, touchMaterial)
	{
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
			GameObject gameObject = _unusedQueue.Dequeue() as GameObject;
			if (!(gameObject == null))
			{
				VespaObject.DestroyGo(gameObject);
			}
		}
		_loadGo = null;
		IsClosed = true;
	}

	public void ReserveComponent(int count)
	{
		if (!_isPooling)
		{
			return;
		}
		int count2 = _unusedQueue.Count;
		if (count2 <= count)
		{
			for (int i = count2; i < count; i++)
			{
				GameObject unusedGo = InstantiateNew(null, -1);
				PutUnused(unusedGo);
			}
		}
	}

	public GameObject Create(Transform parentTm)
	{
		return Create(parentTm, (parentTm == null) ? (-1) : parentTm.gameObject.layer);
	}

	public GameObject Create(Transform parentTm, int layer)
	{
		GameObject gameObject = PopUnused(parentTm, layer);
		if (gameObject == null)
		{
			gameObject = InstantiateNew(parentTm, layer);
		}
		if (gameObject == null)
		{
			return null;
		}
		LinkFactoryComponent.IncreaseLock(gameObject);
		return gameObject;
	}

	internal override void ReleaseInternal(Component comp)
	{
		ReleaseInternal(comp.gameObject);
	}

	internal override void ReleaseInternal(GameObject go)
	{
		if (IsClosed || !_isPooling)
		{
			go.SetActive(value: false);
			go.transform.parent = null;
			VespaObject.DestroyGo(go);
			return;
		}
		UIDragScrollView uIDragScrollView = SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.Get(go.transform);
		if (uIDragScrollView != null)
		{
			uIDragScrollView.scrollView = null;
		}
		PutUnused(go);
	}

	private void PutUnused(GameObject unusedGo)
	{
		_unusedQueue.Enqueue(unusedGo);
		unusedGo.SetActive(value: false);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(unusedGo.transform, _unusedRoot);
	}

	private GameObject PopUnused(Transform parentTm, int layer)
	{
		if (_unusedQueue.Count == 0)
		{
			return null;
		}
		while (_unusedQueue.Count > 0)
		{
			GameObject gameObject = _unusedQueue.Dequeue() as GameObject;
			if (!(gameObject == null))
			{
				if (parentTm != null)
				{
					AdjustLayer(gameObject, layer);
					Vespa_Unity_Transform_Extension_Method.ParentRelative(gameObject.transform, parentTm);
				}
				gameObject.SetActive(value: true);
				return gameObject;
			}
		}
		return null;
	}

	private GameObject InstantiateNew(Transform parentTm, int layer)
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
		AdjustLayer(gameObject, layer);
		SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.GetAuto(gameObject.transform).Link = this;
		return gameObject;
	}

	private void AdjustLayer(GameObject targetGo, int layer)
	{
		if (layer >= 0 && targetGo.layer != layer)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(targetGo, layer);
		}
	}
}
