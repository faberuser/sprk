using System;
using NVespa;

namespace NShared;

public class SnapshotObjectPool<T> where T : class, new()
{
	private static SnapshotObjectPoolImpl<T> _instance;

	private static object _locker = new object();

	public static void InitInstance(Func<T> allocator)
	{
		if (_instance != null)
		{
			return;
		}
		if (allocator == null)
		{
			VespaDebug.LogError("Allocator is null.");
		}
		lock (_locker)
		{
			if (_instance == null)
			{
				_instance = new SnapshotObjectPoolImpl<T>(allocator, 0, 1000);
				SnapshotObjectPoolContainer.PoolList.Add(_instance);
			}
		}
	}

	public static T Alloc()
	{
		if (!SnapshotObjectPoolOption.PoolEnabled)
		{
			return new T();
		}
		if (_instance == null)
		{
			InitInstance(() => new T());
		}
		return _instance.Alloc();
	}

	public static void ReleaseImmediately(T objT)
	{
		if (SnapshotObjectPoolOption.PoolEnabled && _instance != null)
		{
			_instance.ReleaseImmediately(objT);
		}
	}
}
