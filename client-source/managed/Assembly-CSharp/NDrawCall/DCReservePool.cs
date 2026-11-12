using System.Collections.Generic;
using System.Text;
using NVespa;
using UnityEngine;

namespace NDrawCall;

public class DCReservePool : SingletonAuto<DCReservePool>, IReservePool
{
	private int _createCount;

	private int _lastReserveCount;

	private Queue<UIDrawCall> _reserveQueue = new Queue<UIDrawCall>();

	public DCReservePool()
	{
		ReservePoolUtil.AddPool(this);
	}

	public void GetDescription(StringBuilder sb)
	{
		sb.AppendLine("DCReservePool");
		sb.AppendLine($" > Reserve={_reserveQueue.Count}/{_lastReserveCount}");
		sb.AppendLine($" > Create={_createCount}");
	}

	public UIDrawCall Create()
	{
		_createCount++;
		while (_reserveQueue.Count > 0)
		{
			UIDrawCall uIDrawCall = _reserveQueue.Dequeue();
			if (uIDrawCall != null)
			{
				return uIDrawCall;
			}
		}
		return CreateDC();
	}

	public void ClearUnused()
	{
	}

	public void Reserve(bool resetAll)
	{
		_lastReserveCount = Mathf.Max(_createCount, _lastReserveCount) * 2 / 3;
		_createCount = 0;
		if (_lastReserveCount > _reserveQueue.Count)
		{
			int num = _lastReserveCount - _reserveQueue.Count;
			for (int i = 0; i < num; i++)
			{
				_reserveQueue.Enqueue(CreateDC());
			}
		}
	}

	private UIDrawCall CreateDC()
	{
		GameObject gameObject = new GameObject();
		Object.DontDestroyOnLoad(gameObject);
		return gameObject.AddComponent<UIDrawCall>();
	}
}
