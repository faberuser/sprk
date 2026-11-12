using System.Collections.Generic;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset;

public class GameObjectFactory : SceneAutoBehaviour<GameObjectFactory>
{
	private Queue<GameObject> _unusedQueue = new Queue<GameObject>();

	public GameObject Create()
	{
		if (_unusedQueue.Count > 0)
		{
			GameObject obj = _unusedQueue.Dequeue();
			obj.SetActive(value: true);
			return obj;
		}
		return new GameObject();
	}

	private bool EnqueueGameObject(GameObject go)
	{
		go.SetActive(value: false);
		go.transform.parent = base.transform;
		_unusedQueue.Enqueue(go);
		return true;
	}

	public static void ReleaseGameObject(GameObject go)
	{
		if (!SceneAutoBehaviour<GameObjectFactory>.isValidInstance || go.transform.childCount != 0 || !SceneAutoBehaviour<GameObjectFactory>.instance.EnqueueGameObject(go))
		{
			go.SetActive(value: false);
			go.transform.parent = null;
			VespaObject.DestroyGo(go);
		}
	}
}
