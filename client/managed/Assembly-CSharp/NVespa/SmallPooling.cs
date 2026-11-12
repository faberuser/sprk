using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public abstract class SmallPooling<TItem> where TItem : SmallPooling<TItem>, ISmallPoolingItem, new()
{
	internal class SmallPool : SingletonAuto<SmallPool>, ISmallPool
	{
		private Queue<TItem> _unusedQueue = new Queue<TItem>();

		public int UnusedCount { get; set; }

		public int ReserveCount { get; set; }

		public SmallPool()
		{
			SingletonAuto<SmallPoolContainer>.instance.AddPool(this);
		}

		void ISmallPool.OnSceneChanged()
		{
			while (true)
			{
				TItem val = PopUnusedItem();
				if (val == null)
				{
					break;
				}
				val.OnDestroy();
			}
			SingletonAuto<SmallPool>.ReleaseSingleton();
		}

		public void Reserve(int count)
		{
			ReserveCount = Mathf.Max(ReserveCount, count);
			int unusedCount = UnusedCount;
			if (unusedCount < count)
			{
				for (int i = unusedCount; i < count; i++)
				{
					TItem val = new TItem();
					val.OnCreateNew();
					PutUnusedItem(val);
				}
			}
		}

		public TItem Create()
		{
			TItem val = PopUnusedItem();
			if (val == null)
			{
				val = new TItem();
				val.OnCreateNew();
			}
			val._lock++;
			val.OnCreate();
			return val;
		}

		public TItem Capture(TItem item)
		{
			if (item == null)
			{
				return null;
			}
			item._lock++;
			return item;
		}

		public void Release(TItem item)
		{
			if (item != null)
			{
				item._lock--;
				if (item._lock <= 0 && item._lock >= 0)
				{
					item.OnRelease();
					PutUnusedItem(item);
				}
			}
		}

		private TItem PopUnusedItem()
		{
			if (_unusedQueue.Count == 0)
			{
				return null;
			}
			TItem result = _unusedQueue.Dequeue();
			UnusedCount--;
			return result;
		}

		private void PutUnusedItem(TItem item)
		{
			_unusedQueue.Enqueue(item);
			UnusedCount++;
		}
	}

	private int _lock;

	public static void ReserveInstance(int count)
	{
		SingletonAuto<SmallPool>.instance.Reserve(count);
	}

	public static TItem CreateInstance()
	{
		return SingletonAuto<SmallPool>.instance.Create();
	}

	public static TItem CaptureInstance(TItem item)
	{
		return SingletonAuto<SmallPool>.instance.Capture(item);
	}

	public static void ReleaseInstance(TItem instance)
	{
		SingletonAuto<SmallPool>.instance.Release(instance);
	}

	internal SmallPooling()
	{
	}
}
