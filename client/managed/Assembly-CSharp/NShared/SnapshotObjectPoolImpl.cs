using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotObjectPoolImpl<T> : ISnapshotObjectPool where T : class
{
	public const int DefaultListCapacity = 10;

	private Func<T> _allocator;

	private Queue _queue = new Queue();

	private ArrayList _allocatedList = new ArrayList(10);

	private ArrayList _pendingList = new ArrayList(10);

	private ArrayList _tempReleaseList = new ArrayList(10);

	private int _maxPoolSize;

	public SnapshotObjectPoolImpl(Func<T> allocator, int preallocate, int maxPoolSize)
	{
		_allocator = allocator;
		_maxPoolSize = maxPoolSize;
		if (preallocate > 0)
		{
			List<T> list = new List<T>();
			for (int i = 0; i < preallocate; i++)
			{
				list.Add(Alloc());
			}
			for (int j = 0; j < list.Count; j++)
			{
				ReleaseImmediately(list[j]);
			}
			_allocatedList.Clear();
			_pendingList.Clear();
		}
	}

	public void Reset()
	{
		_queue.Clear();
		_allocatedList.Clear();
		_pendingList.Clear();
	}

	public T Alloc()
	{
		T val = null;
		if (_queue.Count > 0)
		{
			val = _queue.Dequeue() as T;
		}
		if (val == null)
		{
			val = _allocator();
		}
		if (val is ISnapshotObject snapshotObject)
		{
			if (snapshotObject.PendingRelease)
			{
				VespaDebug.LogError("PendingRelease is true. " + typeof(T).Name + " value=" + val.GetType().Name);
			}
			snapshotObject.RefCounter.Reset();
			snapshotObject.Pool = this;
			snapshotObject.PendingRelease = false;
			_allocatedList.Add(val);
		}
		else
		{
			VespaDebug.LogError("Allocated type if not SnapshotObject Pool=" + typeof(T).Name + " value=" + val.GetType().Name);
		}
		return val;
	}

	public void ReleaseImmediately(T value)
	{
		if (value != null)
		{
			if (value is ISnapshotObject snapshotObject)
			{
				snapshotObject.OnRelease();
				snapshotObject.PendingRelease = false;
				snapshotObject.Pool = null;
			}
			else
			{
				VespaDebug.LogError("Released type if not SnapshotObject Pool=" + typeof(T).Name + " value=" + value.GetType().Name);
			}
			_queue.Enqueue(value);
		}
	}

	void ISnapshotObjectPool.Release(object value)
	{
		if (value != null && (_maxPoolSize <= 0 || _pendingList.Count <= _maxPoolSize))
		{
			if (value is ISnapshotObject snapshotObject)
			{
				snapshotObject.PendingRelease = true;
				_pendingList.Add(value);
			}
			else
			{
				VespaDebug.LogError("Releaseing type if not SnapshotObject Pool=" + typeof(T).Name + " value=" + value.GetType().Name);
			}
		}
	}

	void ISnapshotObjectPool.FlushPendingRelease()
	{
		_tempReleaseList.Clear();
		for (int i = 0; i < _pendingList.Count; i++)
		{
			if (_pendingList[i] is ISnapshotObject snapshotObject && !snapshotObject.RefCounter.IsAlive() && snapshotObject.Pool != null)
			{
				snapshotObject.Pool = null;
				_tempReleaseList.Add(_pendingList[i]);
			}
		}
		_pendingList.Clear();
		for (int j = 0; j < _allocatedList.Count; j++)
		{
			if (_allocatedList[j] is ISnapshotObject snapshotObject2 && !snapshotObject2.RefCounter.IsAlive() && snapshotObject2.Pool != null)
			{
				snapshotObject2.Pool = null;
				_tempReleaseList.Add(_allocatedList[j]);
			}
		}
		_allocatedList.Clear();
		for (int k = 0; k < _tempReleaseList.Count; k++)
		{
			T value = _tempReleaseList[k] as T;
			ReleaseImmediately(value);
		}
	}
}
