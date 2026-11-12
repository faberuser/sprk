using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NVespa;

public class ReservePool<T> : SingletonAuto<ReservePool<T>>, IReservePool where T : new()
{
	private int _reserveCount;

	private int _createUnusedCount;

	private int _createReservedCount;

	private int _createNewCount;

	private Queue<T> _reserveQueue = new Queue<T>();

	private Queue<T> _unusedQueue = new Queue<T>();

	protected override bool ThreadSafe => true;

	public ReservePool()
	{
		ReservePoolUtil.AddPool(this);
	}

	public void GetDescription(StringBuilder sb)
	{
		sb.AppendLine($"ReservePool<{typeof(T).ToString()}>");
		sb.AppendLine($" > Reserve={_reserveQueue.Count}/{_reserveCount}");
		sb.AppendLine($" > Unused={_unusedQueue.Count}");
		sb.AppendLine($" > Create={_createUnusedCount}, {_createReservedCount}, {_createNewCount}");
		sb.AppendLine();
	}

	public T Create()
	{
		lock (_unusedQueue)
		{
			if (_unusedQueue.Count > 0)
			{
				_createUnusedCount++;
				return _unusedQueue.Dequeue();
			}
		}
		lock (_reserveQueue)
		{
			if (_reserveQueue.Count > 0)
			{
				_createReservedCount++;
				return _reserveQueue.Dequeue();
			}
		}
		_createNewCount++;
		return new T();
	}

	public void Release(T item)
	{
		lock (_unusedQueue)
		{
			_unusedQueue.Enqueue(item);
		}
	}

	public void ReserveByCount(int reserveCount, bool forceReserve = false)
	{
		if (!forceReserve)
		{
			reserveCount = Mathf.Max(reserveCount, (_createReservedCount + _createNewCount) * 2 / 3);
		}
		ReserveInternal(reserveCount);
	}

	public void ClearUnused()
	{
		lock (_unusedQueue)
		{
			_unusedQueue.Clear();
		}
	}

	public void Reserve(bool resetAll)
	{
		int reserveCount = ((!resetAll) ? (Mathf.Max(_reserveCount, _createReservedCount + _createNewCount) * 2 / 3) : (_reserveCount * 2 / 3));
		ReserveInternal(reserveCount);
	}

	private void ReserveInternal(int reserveCount)
	{
		_reserveCount = reserveCount;
		_createUnusedCount = 0;
		_createReservedCount = 0;
		_createNewCount = 0;
		lock (_reserveQueue)
		{
			if (_reserveCount > _reserveQueue.Count)
			{
				int num = _reserveCount - _reserveQueue.Count;
				for (int i = 0; i < num; i++)
				{
					_reserveQueue.Enqueue(new T());
				}
			}
		}
	}
}
