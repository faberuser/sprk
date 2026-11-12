namespace NShared;

public static class SnapshotMember_ExtensionMethod
{
	public static T[] GET<T>(this SnapshotObjectMember<SnapshotArray<T>> member)
	{
		return member?.Get()?.Get();
	}
}
