namespace NShared;

public class SnapshotObjectMember<T> : ISnapshotMember
{
	private T _data;

	private readonly ISnapshotMemberSerializer _impl;

	static SnapshotObjectMember()
	{
	}

	public SnapshotObjectMember(ISnapshotParent parent, T value = default(T), ISnapshotMemberSerializer impl = null)
	{
		_impl = impl ?? SnapshotMemberImplFactory.GetImpl<T>();
		Set(value);
		parent?.AddMember(this);
	}

	public T Set(T data)
	{
		SnapshotWritableBlock.CheckSnapshotWritable();
		T data2 = _data;
		_data = data;
		OnChanged(data2, data);
		return data;
	}

	private void OnChanged(T before, T after)
	{
		SnapshotUtility.IncRefCount(after);
		SnapshotUtility.DecRefCount(before);
	}

	public void Reset()
	{
		SnapshotWritableBlock.CheckSnapshotWritable();
		Set(default(T));
	}

	public T Get()
	{
		return _data;
	}

	void ISnapshotMember.Set(object data)
	{
		Set((T)data);
	}

	object ISnapshotMember.Get()
	{
		return Get();
	}

	ISnapshotMemberSerializer ISnapshotMember.GetSerializer()
	{
		return _impl;
	}

	public static implicit operator T(SnapshotObjectMember<T> snapshot)
	{
		return snapshot.Get();
	}
}
