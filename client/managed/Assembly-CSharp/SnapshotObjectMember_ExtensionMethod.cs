using NShared;

public static class SnapshotObjectMember_ExtensionMethod
{
	public static T Alloc<T>(this SnapshotObjectMember<T> snapshotMember) where T : class, new()
	{
		T data = SnapshotObjectPool<T>.Alloc();
		return snapshotMember.Set(data);
	}
}
