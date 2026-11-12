using System.Threading;

namespace NShared;

public static class SnapshotUidAllocator
{
	private static long _uid;

	public static long Allocate()
	{
		return Interlocked.Increment(ref _uid);
	}
}
