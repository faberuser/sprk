using System.Collections.Generic;

namespace NShared;

public class SnapshotObjectPoolContainer
{
	public static List<ISnapshotObjectPool> PoolList = new List<ISnapshotObjectPool>();

	public static void Reset()
	{
		lock (PoolList)
		{
			for (int i = 0; i < PoolList.Count; i++)
			{
				PoolList[i].Reset();
			}
		}
	}

	public static void FlushPendingRelease()
	{
		lock (PoolList)
		{
			for (int i = 0; i < PoolList.Count; i++)
			{
				PoolList[i].FlushPendingRelease();
			}
		}
	}
}
