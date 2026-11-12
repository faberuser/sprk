using System.Collections.Generic;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NUtil;

public class GameObjectDestroyer : GlobalAutoBehaviour<GameObjectDestroyer>
{
	private static object _lockObj = new object();

	private static Queue<GameObject> _curQueue = new Queue<GameObject>();

	private static Queue<GameObject> _nextQueue = new Queue<GameObject>();

	private static Queue<GameObject> _destroyQueue = new Queue<GameObject>();

	public bool ShouldClearCacheOnPause { get; set; }

	public static void PendingDestroy(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		go.SetActive(value: false);
		if (go.transform != null)
		{
			go.transform.parent = GlobalAutoBehaviour<GameObjectDestroyer>.instance.transform;
		}
		lock (_lockObj)
		{
			_curQueue.Enqueue(go);
		}
	}

	private void LateUpdate()
	{
		Queue<GameObject> curQueue = _curQueue;
		lock (_lockObj)
		{
			_curQueue = _destroyQueue;
		}
		_destroyQueue = _nextQueue;
		_nextQueue = curQueue;
		while (_destroyQueue.Count > 0)
		{
			VespaObject.DestroyObject(_destroyQueue.Dequeue());
		}
	}
}
